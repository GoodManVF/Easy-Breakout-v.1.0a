# Easy-Breakout-v.1.0a

This robot is able to trade exchange assets in automatic mode, makes decisions when the price leaves the range of the Asian session

He waits until the end of the Asian session, at the end he determines the minimum and maximum prices for this period, 
draws levels on the chart, and then, if the price crosses the upper or lower range, a trade of the corresponding direction (buy/sell) is opened.

The stop loss of the trade is set beyond the opposite level of the range, the take profit is 1.5 times higher.
All these parameters, as well as others, can be changed by the user interface before starting

Of the available options - when the volatility is low, or when the volatility is too high, it does not open trades.
If the take profit does not work, the trade will close at the end of the day
