using System;
using System.Linq;
using cAlgo.API;
using System.Collections.Generic;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Globalization;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EEuropeStandardTime, AccessRights = AccessRights.None)]


    public class EasyBreakout : Robot
    {
        [Parameter("Select trade volume ", DefaultValue = "2", MinValue = "0.01", MaxValue = "10000")]
        public double SelectLot { get; set; }

        [Parameter("Select trigger distance (pips) ", DefaultValue = "70", MinValue = "1", MaxValue = "300")]
        public double TriggerDistance { get; set; }

        [Parameter("Profit trigger (pips) ", DefaultValue = "120", MinValue = "0", MaxValue = "700")]
        public double ProfitTrigger { get; set; }

        [Parameter("Brake Even SL (pips) ", DefaultValue = "20", MinValue = "0", MaxValue = "350")]
        public double BrakeEvenSL { get; set; }

        [Parameter("1st TP (pips) ", DefaultValue = "350", MinValue = "0", MaxValue = "500")]
        public double FirstTp { get; set; }

        [Parameter("2nd TP (pips) ", DefaultValue = "700", MinValue = "0", MaxValue = "1000")]
        public double SecondTp { get; set; }

        TimeSpan countStart = TimeSpan.ParseExact("01:59", "hh\\:mm", null);
        TimeSpan countStop = TimeSpan.ParseExact("09:00", "hh\\:mm", null);
        TimeSpan nullTime = TimeSpan.ParseExact("01:00", "hh\\:mm", null);


        //double HightestBarPrice;
        //double LowestBarPrice;
        //int index = 0;
        bool IsInRange;
        bool CanOpen;
        double MaxHigh;
        double MinLow;
        double BuyStopPrice;
        double SellStopPrice;
        int buyFilled = 0;
        int sellFilled = 0;
        double buyEndRange;
        double sellEndRange;


        protected override void OnStart()
        {
            Print("Started..");
            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionsClosed;
            //PendingOrders.
            IsInRange = true;
        }

        private void OnPositionsOpened(PositionOpenedEventArgs args)
        {
            var position = args.Position;
            if (position.Label == "BuyStop")
            {

                Print("ID: {0} - Opened new {1} position, type {2}, at entry price {3}, with volume: {4}", position.Id, position.SymbolName, position.TradeType, position.EntryPrice, position.Quantity);
                buyFilled++;
            }
            else if (position.Label == "SellStop")
            {

                Print("ID: {0} - Opened new {1} position, type {2}, at entry price {3}, with volume: {4}", position.Id, position.SymbolName, position.TradeType, position.EntryPrice, position.Quantity);
                sellFilled++;
            }
        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {
            var position = args.Position;
            var type = args.Position.TradeType;
            var volume = args.Position.Quantity;

            if (args.Reason == PositionCloseReason.Closed & type == TradeType.Buy)
            {
                Print("ID: {0} - Closed {1} position by CLOSE, type {2}, at entry price {3}, with volume: {4}", position.Id, position.SymbolName, position.TradeType, position.EntryPrice, position.VolumeInUnits);
                //buyFilled--;
                //buyStep = 0;

            }
            else if (args.Reason == PositionCloseReason.StopLoss & type == TradeType.Buy)
            {
                //buyFilled--;
                //buyStep = 0;
            }
            else if (args.Reason == PositionCloseReason.TakeProfit & type == TradeType.Buy)
            {
                Print("ID: {0} - Closed {1} position by TP, type {2}, at entry price {3}, with volume: {4}", position.Id, position.SymbolName, position.TradeType, position.EntryPrice, position.Quantity);
                //buyFilled--;
                //buyStep = 0;
            }
            else if (args.Reason == PositionCloseReason.Closed & type == TradeType.Sell)
            {
                //sellFilled--;
                //SellStep = 0;
            }
            else if (args.Reason == PositionCloseReason.StopLoss & type == TradeType.Sell)
            {
                //sellFilled--;
                //SellStep = 0;
            }
            else if (args.Reason == PositionCloseReason.TakeProfit & type == TradeType.Sell)
            {
                //sellFilled--;
                //SellStep = 0;
            }
        }

        public List<double> listOfPrices = new List<double> 
        {

                    };

        protected override void OnBar()
        {

            var currentHour = Math.Round(Server.Time.TimeOfDay.TotalHours);

            if (Server.Time.TimeOfDay >= countStart && Server.Time.TimeOfDay <= countStop)
            {
                IsInRange = true;
                Print("Calculate levels..");
                // Print("Last 1 Bar High: " + Bars.HighPrices.Last(1) + "\tOpen Las 1: " + Bars.OpenTimes.Last(1));
                listOfPrices.Add(Bars.HighPrices.Last(1));
                listOfPrices.Add(Bars.LowPrices.Last(1));
            }
            else
            {
                IsInRange = false;
                //Print("Not in range");
                try
                {
                    double max = listOfPrices.Max();
                    double min = listOfPrices.Min();
                    MaxHigh = max;
                    MinLow = min;
                    BuyStopPrice = MaxHigh + TriggerDistance * Symbol.PipSize;
                    SellStopPrice = MinLow - TriggerDistance * Symbol.PipSize;

                    //Print("Max price is: " + max + "\tMin price is: " + min);
                } catch (InvalidOperationException)
                {
                    Print("No data for draw levels..Please wait..");

                    //throw;
                }

            }
            if (currentHour == 1)
            {
                listOfPrices.Clear();
                Print("Table cleaned..");
                buyFilled = 0;
                sellFilled = 0;
            }

            if (!IsInRange && currentHour <= 23)
            {

                Chart.DrawHorizontalLine("Max price high", MaxHigh, Color.DimGray);
                Chart.DrawHorizontalLine("Max price low", MinLow, Color.DimGray);

                Chart.DrawHorizontalLine("Buy stop price", BuyStopPrice, Color.Blue);
                Chart.DrawHorizontalLine("Sell stop price", SellStopPrice, Color.Crimson);
            }
            else
            {
                Chart.RemoveObject("Max price high");
                Chart.RemoveObject("Max price low");

                Chart.RemoveObject("Buy stop price");
                Chart.RemoveObject("Sell stop price");
            }

        }

        //Open positions
        protected override void OnTick()
        {
            buyEndRange = BuyStopPrice + 100 * Symbol.PipSize;
            sellEndRange = SellStopPrice - 100 * Symbol.PipSize;
            if (Symbol.Ask >= BuyStopPrice && Symbol.Ask <= buyEndRange && !IsInRange && buyFilled < 1)
            {
                ExecuteMarketRangeOrder(TradeType.Buy, Symbol.Name, SelectLot, 10, Symbol.Ask, "BuyStop", 350, SecondTp);
                Print("Range: " + 200 * Symbol.PipSize);
            }
            if (Symbol.Bid <= SellStopPrice && Symbol.Bid >= sellEndRange && !IsInRange && sellFilled < 1)
            {
                ExecuteMarketRangeOrder(TradeType.Sell, Symbol.Name, SelectLot, 10, Symbol.Bid, "SellStop", 350, SecondTp);
            }
            //Print("Buys: " + buyFilled + " Sells: " + sellFilled);
            //Breakeven & SL Levels

            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy)
                {
                    if (position.Pips >= ProfitTrigger)
                    {
                        double newStopLoss = position.EntryPrice + BrakeEvenSL * Symbol.PipSize;
                        ModifyPosition(position, newStopLoss, null);

                    }
                    if (position.Pips >= FirstTp & position.Quantity == SelectLot)
                    {
                        ClosePosition(position, SelectLot / 2);

                    }
                    if (position.Pips >= SecondTp & position.Quantity == SelectLot / 2)
                    {
                        ClosePosition(position);

                    }

                }
                if (position.TradeType == TradeType.Sell)
                {
                    if (position.Pips >= ProfitTrigger)
                    {
                        double newStopLoss = position.EntryPrice - BrakeEvenSL * Symbol.PipSize;
                        ModifyPosition(position, newStopLoss, null);

                    }
                    if (position.Pips >= FirstTp & position.Quantity == SelectLot)
                    {
                        ClosePosition(position, SelectLot / 2);

                    }
                    if (position.Pips >= SecondTp & position.Quantity == SelectLot / 2)
                    {
                        ClosePosition(position);


                    }
                }
                if (IsInRange)
                {
                    ClosePosition(position);

                }
            }


        }



        protected override void OnStop()
        {
            foreach (var position in Positions)
            {
                ClosePosition(position);
            }
        }
    }
}
