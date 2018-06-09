﻿/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：MemcachedContainerStrategy.cs
    文件功能描述：Memcached 容器缓存策略。


    创建标识：Senparc - 20160308

    修改标识：Senparc - 20160808
    修改描述：v0.0.2 删除 ItemCollection 属性，直接使用ContainerBag加入到缓存

    修改标识：Senparc - 20160812
    修改描述：v0.0.3  解决Container无法注册的问题

    修改标识：Senparc - 20160812
    修改描述：v0.0.5  添加ServerList配制方法

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.Weixin.Containers;
#if NET45 || NET461

#else
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#endif

namespace Senparc.Weixin.Cache.Memcached
{
    public class MemcachedContainerStrategy : IContainerCacheStrategy
    {
        #region IDomainExtensionCacheStrategy 成员
        public ICacheStrategyDomain CacheStrategyDomain { get { return ContainerCacheStrategyDomain.Instance; } }

        /// <summary>
        /// 数据源缓存策略
        /// </summary>
        public Func<IBaseObjectCacheStrategy> BaseCacheStrategy { get; }


        #endregion


        #region 单例
        /// <summary>
        /// LocalCacheStrategy的构造函数
        /// </summary>
        MemcachedContainerStrategy()
        {
            //使用底层缓存策略
            BaseCacheStrategy = () => MemcachedObjectCacheStrategy.Instance;

            //向底层缓存注册当前缓存策略
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(this);
        }

        //静态LocalCacheStrategy
        public static new IContainerCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly MemcachedContainerStrategy instance = new MemcachedContainerStrategy();
        }
        #endregion


        #region IContainerCacheStrategy 成员

        public IDictionary<string, TBag> GetAll<TBag>() where TBag : IBaseContainerBag
        {
            throw new NotImplementedException();
        }

        public void UpdateContainerBag(string key, IBaseContainerBag containerBag, bool isFullKey = false)
        {
            var baseCacheStrategy = BaseCacheStrategy();
            object value;
            if ((baseCacheStrategy as MemcachedObjectCacheStrategy).TryGet(key, out value))
            {
                baseCacheStrategy.Update(key, containerBag, true);
            }
        }


        #endregion

    }
}
