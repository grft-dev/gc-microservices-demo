// Copyright 2018 Google LLC
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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace cartservice.cartstore
{
    public class RedisCartStore : ICartStore
    {
        private readonly IDistributedCache _cache;

        public RedisCartStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task AddItemAsync(string userId, string productId, int quantity)
        {
            try
            {
                var cart = await GetCartAsync(userId);
                var existingItem = cart.Items.SingleOrDefault(i => i.ProductId == productId);
                if (existingItem == null)
                {
                    cart.Items.Add(new CartItemSnapshot { ProductId = productId, Quantity = quantity });
                }
                else
                {
                    existingItem.Quantity += quantity;
                }

                cart.UserId = userId;
                await _cache.SetAsync(Key(userId), Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cart)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't access cart storage. {ex.Message}", ex);
            }
        }

        public async Task EmptyCartAsync(string userId)
        {
            try
            {
                await _cache.RemoveAsync(Key(userId));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't access cart storage. {ex.Message}", ex);
            }
        }

        public async Task<CartSnapshot> GetCartAsync(string userId)
        {
            try
            {
                var value = await _cache.GetAsync(Key(userId));
                if (value == null)
                {
                    return new CartSnapshot();
                }

                var cart = JsonSerializer.Deserialize<CartSnapshot>(Encoding.UTF8.GetString(value));
                return cart ?? new CartSnapshot();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't access cart storage. {ex.Message}", ex);
            }
        }

        public bool Ping()
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string Key(string userId) => $"cart:{userId}";
    }
}
