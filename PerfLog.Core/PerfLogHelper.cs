using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PerfLog.Core
{
    /// <summary>
    /// 在Global的Application_BeginRequest里调用.Init()。
    /// 在要记日志的地方调用.Attach()
    /// </summary>
    public static partial class PerfLogHelper
    {
        #region Init Clear

        /// <summary>
        /// 性能日志默认存放位置，HttpContext.Current.Items[PerfLogDefaultContextName]
        /// </summary>
        public static string PerfLogDefaultContextName { get; } = "PerfLog.DefaultContextName";

        /// <summary>
        /// 可能会创建别的组临时存放性能日志，创建组时可能有并行出现导致创建多个Key，所以创建时使用锁来保证只创建一个。
        /// 内部是这么用的：lock(HttpContext.Current.Items[CurrentContextLockName])
        /// </summary>
        private static string CurrentContextLockName { get; } = "PerfLog.CurrentContextLockName";

        /// <summary>
        /// 记日志时有一种方法是：只写个名称，而上一次的时间是记录在：
        /// HttpContext.Current.Items[CurrentContextLastRecordTimeName]
        /// </summary>
        private static string CurrentContextLastRecordTimeName { get; } = "PerfLog.CurrentContextLastRecordTimeName";


        /// <summary>
        /// 一般必须且只能调用一次，在Global里的Application_BeginRequest里。
        /// </summary>
        public static void Init()
        {
            if (mockHttpContent.Count > 1000)
            {
                //应该不会命中此场景,防止其导致内存暴涨，所以清空
                mockHttpContent.Clear();
            }

            ContextItems[PerfLogDefaultContextName] = new List<PerfLogNode>();
            ContextItems[CurrentContextLockName] = new object();
            ContextItems[CurrentContextLastRecordTimeName] = DateTime.Now;
        }

        public static void Clear(Guid? group = null)
        {
            GetContextItem(group).Clear();
        }

        #endregion


        #region Attach

        /// <summary>
        /// 将性能日志附加到上下文
        /// 如果其它几个Attach你搞不明白时，请使用这个。
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="group">设置后会临时存放，最终要调用.Attach(group)，以附加到上下文</param>
        /// <returns></returns>
        public static PerfLogNode Attach(string Name, double Value, Guid? group = null)
        {
            var node = new PerfLogNode { Name = Name, Value = (int)Value };

            var items = GetContextItem(group);

            items.Add(node);

            return node;
        }


        /// <summary>
        /// 将性能日志附加到上下文
        /// 耗时算法：(DateTime.Now - beginTime).TotalMilliseconds
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="beginTime"></param>
        /// <param name="group">设置后会临时存放，最终要调用.Attach(group)，以附加到上下文</param>
        /// <returns></returns>
        public static PerfLogNode Attach(string Name, DateTime beginTime, Guid? group = null)
        {
            return Attach(Name, (DateTime.Now - beginTime).TotalMilliseconds, group);
        }

        /// <summary>
        /// 将性能日志附加到上下文
        /// 如果当前上下文里有并发时，请针对不同并发指定不同的group，并最终调用.Attach(group)
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="group">设置后会临时存放，最终要调用.Attach(group)，以附加到上下文</param>
        /// <returns></returns>
        public static PerfLogNode Attach(string Name, Guid? group = null)
        {
            DateTime newTime = DateTime.Now;
            DateTime oldTime;


            var groupName = CurrentContextLastRecordTimeName;
            if (group.HasValue)
            {
                groupName = $"{CurrentContextLastRecordTimeName}.{group}";
                if (!ContextItems.Contains(groupName))
                {
                    lock (ContextItems[CurrentContextLockName]) //TODO:要验证是否真的锁了
                    {
                        if (!ContextItems.Contains(groupName))
                        {
                            oldTime = DateTime.Now;
                            ContextItems[groupName] = oldTime;
                        }
                        else
                        {
                            oldTime = (DateTime)ContextItems[groupName];
                        }
                    }
                }
                else
                {
                    oldTime = (DateTime)ContextItems[groupName];
                }
            }
            else
            {
                oldTime = (DateTime)ContextItems[CurrentContextLastRecordTimeName];
            }

            ContextItems[groupName] = newTime;


            return Attach(Name, (newTime - oldTime).TotalMilliseconds, group);
        }

        /// <summary>
        /// 将组里的内容附加到上下文
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static void Attach(Guid group)
        {
            var groupName = group.ToString();
            if (ContextItems.Contains(groupName))
            {
                var groupItems = ContextItems[groupName] as List<PerfLogNode>;

                var items = GetContextItem();
                items.AddRange(groupItems);

                groupItems.Clear();
            }
        }

        #endregion


        #region ContextItems ContextItem

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="group">null表示拿默认group</param>
        /// <returns></returns>
        public static List<PerfLogNode> GetContextItem(Guid? group = null)
        {
            List<PerfLogNode> items;

            if (group.HasValue)
            {
                var groupName = group.Value.ToString();
                if (!ContextItems.Contains(groupName))
                {
                    lock (ContextItems[CurrentContextLockName]) //TODO:要验证是否真的锁了
                    {
                        if (!ContextItems.Contains(groupName))
                        {
                            items = new List<PerfLogNode>();
                            ContextItems[groupName] = items;
                        }
                        else
                        {
                            items = ContextItems[groupName] as List<PerfLogNode>;
                        }
                    }
                }
                else
                {
                    items = ContextItems[groupName] as List<PerfLogNode>;
                }
            }
            else
            {
                items = ContextItems[PerfLogDefaultContextName] as List<PerfLogNode>;
            }

            return items;
        }

        private static IDictionary mockHttpContent = new Dictionary<object, object>();
        public static IDictionary ContextItems
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items;
                }
                else
                {
                    return mockHttpContent;
                }
            }
        }

        #endregion
    }

}
