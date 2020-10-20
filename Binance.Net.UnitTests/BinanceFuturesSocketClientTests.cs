﻿using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects;
using Binance.Net.UnitTests.TestImplementations;
using NUnit.Framework;
using CryptoExchange.Net.Logging;

namespace Binance.Net.UnitTests
{
    [TestFixture()]
    public class BinanceFuturesSocketTest
    {
        [TestCase()]
        public void SubscribingToKlineStream_Should_TriggerWhenKlineStreamMessageIsReceived()
        {
            // arrange
            var socket = new TestSocket();
            var client = TestHelpers.CreateFuturesSocketClient(socket);

            BinanceStreamKlineData result = null;
            client.SubscribeToKlineUpdatesAsync("ETHBTC", KlineInterval.OneMinute, (test) => result = test);

            var data = new BinanceCombinedStream<BinanceStreamKlineData>()
            {
                Stream = "test",
                Data = new BinanceStreamKlineData()
                {
                    Event = "TestKlineStream",
                    EventTime = new DateTime(2017, 1, 1),
                    Symbol = "ETHBTC",
                    Data = new BinanceStreamKline()
                    {
                        TakerBuyBaseAssetVolume = 0.1m,
                        Close = 0.2m,
                        CloseTime = new DateTime(2017, 1, 2),
                        Final = true,
                        FirstTrade = 10000000000,
                        High = 0.3m,
                        Interval = KlineInterval.OneMinute,
                        LastTrade = 2000000000000,
                        Low = 0.4m,
                        Open = 0.5m,
                        TakerBuyQuoteAssetVolume = 0.6m,
                        QuoteAssetVolume = 0.7m,
                        OpenTime = new DateTime(2017, 1, 1),
                        Symbol = "test",
                        TradeCount = 10,
                        Volume = 0.8m
                    }
                }
            };

            // act
            socket.InvokeMessage(data);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(TestHelpers.AreEqual(data.Data, result, "Data"));
            Assert.IsTrue(TestHelpers.AreEqual(data.Data.Data, result.Data));
        }

        [TestCase()]
        public void SubscribingToSymbolTicker_Should_TriggerWhenSymbolTickerStreamMessageIsReceived()
        {
            // arrange
            var socket = new TestSocket();
            var client = TestHelpers.CreateFuturesSocketClient(socket);

            BinanceStreamTick result = null;
            client.SubscribeToSymbolTickerUpdates("ETHBTC", (test) => result = test);

            var data = new BinanceStreamTick()
            {
                BestAskPrice = 0.1m,
                BestAskQuantity = 0.2m,
                BestBidPrice = 0.3m,
                BestBidQuantity = 0.4m,
                CloseTradesQuantity = 0.5m,
                CurrentDayClosePrice = 0.6m,
                FirstTradeId = 1,
                HighPrice = 0.7m,
                LastTradeId = 2,
                LowPrice = 0.8m,
                OpenPrice = 0.9m,
                PrevDayClosePrice = 1.0m,
                PriceChange = 1.1m,
                PriceChangePercentage = 1.2m,
                StatisticsCloseTime = new DateTime(2017, 1, 2),
                StatisticsOpenTime = new DateTime(2017, 1, 1),
                Symbol = "test",
                TotalTradedBaseAssetVolume = 1.3m,
                TotalTradedQuoteAssetVolume = 1.4m,
                TotalTrades = 3,
                WeightedAverage = 1.5m
            };

            // act
            socket.InvokeMessage(data);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(TestHelpers.AreEqual(data, result));
        }

        [TestCase()]
        public void SubscribingToUserStream_Should_TriggerWhenAccountUpdateStreamMessageIsReceived()
        {
            // arrange
            var socket = new TestSocket();
            var client = TestHelpers.CreateFuturesSocketClient(socket);

            BinanceFuturesStreamAccountInfo result = null;
            client.SubscribeToUserDataUpdates("test", (test) => result = test, null, null, null, null, null);

            var data = new BinanceFuturesStreamAccountInfo()
            {
                Event = "outboundAccountInfo",
                EventTime = new DateTime(2017, 1, 1),
                Positions = new List<BinanceFuturesStreamPosition>(),
                Balances = new List<BinanceFuturesStreamBalance>()
                {
                    new BinanceFuturesStreamBalance(){ Asset = "test1", WalletBalance = 1.1m},
                    new BinanceFuturesStreamBalance(){ Asset = "test2", WalletBalance = 3.3m},
                }
            };

            // act
            socket.InvokeMessage(data);

            // assert
            Assert.IsNotNull(result);
            var expectedBalances = data.Balances.ToList();
            var balances = result.Balances.ToList();
            Assert.IsTrue(TestHelpers.AreEqual(data, result, "Balances"));
            Assert.IsTrue(TestHelpers.AreEqual(expectedBalances[0], balances[0]));
            Assert.IsTrue(TestHelpers.AreEqual(expectedBalances[1], balances[1]));
        }

        [TestCase()]
        public void SubscribingToUserStream_Should_TriggerWhenOcoOrderUpdateStreamMessageIsReceived()
        {
            // arrange
            var socket = new TestSocket();
            var client = TestHelpers.CreateFuturesSocketClient(socket, new BinanceFuturesSocketClientOptions(){ LogVerbosity = LogVerbosity.Debug });

            BinanceStreamOrderList result = null;
            client.SubscribeToUserDataUpdatesAsync("test", null, null, (test) => result = test, null, null, null);

            var data = new BinanceStreamOrderList()
            {
                Event = "listStatus",
                EventTime = new DateTime(2017, 1, 1),
                Symbol = "BNBUSDT",
                ContingencyType = "OCO",
                ListStatusType = ListStatusType.Done,
                ListOrderStatus = ListOrderStatus.Done,
                OrderListId = 1,
                ListClientOrderId = "2",
                TransactionTime = new DateTime(2018, 1, 1),
                Orders = new []
                {
                    new BinanceStreamOrderId()
                    {
                        Symbol = "BNBUSDT",
                        OrderId = 2,
                        ClientOrderId = "3"
                    },
                    new BinanceStreamOrderId()
                    {
                        Symbol = "BNBUSDT",
                        OrderId = 3,
                        ClientOrderId = "4"
                    }
                }
            };

            // act
            socket.InvokeMessage(data);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(TestHelpers.AreEqual(data, result, "Orders"));
            Assert.IsTrue(TestHelpers.AreEqual(data.Orders.ToList()[0], result.Orders.ToList()[0]));
            Assert.IsTrue(TestHelpers.AreEqual(data.Orders.ToList()[1], result.Orders.ToList()[1]));
        }

        [TestCase()]
        public void SubscribingToUserStream_Should_TriggerWhenOrderUpdateStreamMessageIsReceived()
        {
            // arrange
            var socket = new TestSocket();
            var client = TestHelpers.CreateFuturesSocketClient(socket);

            BinanceFuturesStreamOrderUpdate result = null;
            client.SubscribeToUserDataUpdatesAsync("test", null, (test) => result = test, null, null, null, null);

            var data = new BinanceFuturesStreamOrderUpdate()
            {
                Event = "executionReport",
                EventTime = new DateTime(2017, 1, 1),
                AccumulatedQuantityOfFilledTrades = 1.1m,
                AveragePrice = 3.3m,
                BuyerIsMaker = true,
                Commission = 2.2m,
                CommissionAsset = "test",
                ExecutionType = ExecutionType.Trade,
                OrderId = 100000000000,
                Price = 6.6m,
                PriceLastFilledTrade = 7.7m,
                Quantity = 8.8m,
                QuantityOfLastFilledTrade = 9.9m,
                Side = OrderSide.Buy,
                Status = OrderStatus.Filled,
                Symbol = "test",
                OrderCreationTime = new DateTime(2017, 1, 1),
                TimeInForce = TimeInForce.GoodTillCancel,
                TradeId = 10000000000000,
                Type = OrderType.Limit,
                ClientOrderId = "123",
                StopPrice = 10.10m
            };

            // act
            socket.InvokeMessage(data);

            // assert
            Assert.IsNotNull(result);
            Assert.IsTrue(TestHelpers.AreEqual(data, result, "Balances"));
        }
    }
}