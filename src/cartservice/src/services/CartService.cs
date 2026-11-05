// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Globalization;
using System.Linq;
using cartservice.cartstore;
using cartservice.Graftcode;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace cartservice.services
{
    public static class CartService
    {
        public static string AddItem(string userId, string productId, int quantity) =>
            GraftAsync.Run(async () =>
            {
                var store = CreateStore();
                await store.AddItemAsync(userId, productId, quantity);

                var cart = await store.GetCartAsync(userId);
                var item = cart.Items.Single(i => i.ProductId == productId);
                return $"{Escape(item.ProductId)}|{item.Quantity.ToString(CultureInfo.InvariantCulture)}";
            });

        public static string[] GetCart(string userId) =>
            GraftAsync.Run(async () =>
            {
                var cart = await CreateStore().GetCartAsync(userId);
                return cart.Items
                    .OrderBy(item => item.ProductId, StringComparer.Ordinal)
                    .Select(item => $"{Escape(item.ProductId)}|{item.Quantity.ToString(CultureInfo.InvariantCulture)}")
                    .ToArray();
            });

        public static string EmptyCart(string userId) =>
            GraftAsync.Run(async () =>
            {
                await CreateStore().EmptyCartAsync(userId);
                return string.Empty;
            });

        private static ICartStore CreateStore()
        {
            var redisAddress = Environment.GetEnvironmentVariable("REDIS_ADDR");
            IDistributedCache cache;
            if (string.IsNullOrWhiteSpace(redisAddress))
            {
                cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            }
            else
            {
                cache = new RedisCache(Options.Create(new RedisCacheOptions { Configuration = redisAddress }));
            }

            return new RedisCartStore(cache);
        }

        private static string Escape(string value) =>
            value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("|", "\\|", StringComparison.Ordinal);
    }
}