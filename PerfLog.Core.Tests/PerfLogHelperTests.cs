using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerfLog.Core.Extend;

/// <summary>
/// 如果是在这些环境：webform,mvc,webapi，IIS宿主，天生的多线程，测试没问题。
/// ★★★★★此处的测试用例都算是winform，只能同时运行一个测试用例
/// </summary>
namespace PerfLog.Core.Tests
{
    //因为程序的特殊性，同时只能有一个测试用例在跑，否则不准确或有问题。
    [TestClass]
    public class PerfLogHelperTests
    {
        static PerfLogHelperTests()
        {
            PerfLogHelper.Init();
        }

        public string GetContextStr()
        {
            return PerfLogHelper.GetContextItem().ToJson(isNeedFormat: true, isCanCyclicReference: false, ignoreNullValue: true);
        }


        [TestMethod]
        public void Attach_Test_OnlyName()
        {
            PerfLogHelper.Clear();
            PerfLogHelper.Attach("开始");
            Thread.Sleep(1000);
            PerfLogHelper.Attach("查酒店结束");

            //调用具体接口
            //拿到上下文
            var children = @"[
                                {
                                'N': 'SearchLocalFare',
                                'V': 123
                                },
                                {
                                'N': 'SearchGDS',
                                'V': 234
                                },
                                {
                                'N': '合并',
                                'V': 456
                                },
                                {
                                'N': '总耗时',
                                'V': 789
                                }
                            ]".FromJson<List<PerfLogNode>>();

            //记录
            var node = PerfLogHelper.Attach("查机票结束");
            node.Children = children;

            Thread.Sleep(100);
            PerfLogHelper.Attach("查可选项结束");
            var actual = GetContextStr();
        }






        [TestMethod]
        public void Attach_Test_Name_BeginTime()
        {
            PerfLogHelper.Clear();
            var timer = DateTime.Now.AddMilliseconds(-900000);
            PerfLogHelper.Attach("开始", timer);
            Thread.Sleep(1000);
            PerfLogHelper.Attach("查酒店结束", timer);

            Thread.Sleep(2000);
            PerfLogHelper.Attach("查机票结束", timer);

            Thread.Sleep(100);
            PerfLogHelper.Attach("查可选项结束", timer);

            timer = DateTime.Now;

            Thread.Sleep(2000);
            PerfLogHelper.Attach("1.查机票结束", timer);

            Thread.Sleep(100);
            PerfLogHelper.Attach("1.查可选项结束", timer);
            var actual = GetContextStr();
        }


        [TestMethod]
        public void Attach_Test_Name_Value()
        {

            PerfLogHelper.Clear();
            var timer = DateTime.Now;

            PerfLogHelper.Attach("开始", (DateTime.Now - timer).TotalMilliseconds);

            Thread.Sleep(1000);
            PerfLogHelper.Attach("查酒店结束", (DateTime.Now - timer).TotalMilliseconds);


            Thread.Sleep(2000);
            PerfLogHelper.Attach("查机票结束", (DateTime.Now - timer).TotalMilliseconds);

            Thread.Sleep(100);
            PerfLogHelper.Attach("查可选项结束", (DateTime.Now - timer).TotalMilliseconds);

            var actual = GetContextStr();


        }

        /// <summary>
        /// 有组，并发时日志很整齐
        /// </summary>

        [TestMethod]
        public void Attach_Test_Parallel_Name_Group()
        {
            PerfLogHelper.Clear();
            Parallel.Invoke(
                () =>
                {
                    var timer = DateTime.Now;
                    var guid = Guid.NewGuid();
                    PerfLogHelper.Attach("组1.开始", guid);
                    Thread.Sleep(1000);
                    PerfLogHelper.Attach("组1.查酒店结束", guid);

                    Thread.Sleep(2000);
                    PerfLogHelper.Attach("组1.查机票结束", guid);

                    Thread.Sleep(100);
                    PerfLogHelper.Attach("组1.查可选项结束", guid);
                    PerfLogHelper.Attach(guid);

                },
                () =>
                {
                    var timer = DateTime.Now;
                    var guid = Guid.NewGuid();
                    PerfLogHelper.Attach("组2.开始", guid);
                    Thread.Sleep(1000);
                    PerfLogHelper.Attach("组2.查酒店结束", guid);

                    Thread.Sleep(2000);
                    PerfLogHelper.Attach("组2.查机票结束", guid);

                    Thread.Sleep(100);
                    PerfLogHelper.Attach("组2.查可选项结束", guid);
                    PerfLogHelper.Attach(guid);
                }
                );

            var actual = GetContextStr();

        }

        /// <summary>
        /// 并发时，没有组，日志很乱
        /// </summary>
        [TestMethod]
        public void Attach_Test_Parallel_Name_NoGroup()
        {
            PerfLogHelper.Clear();
            Parallel.Invoke(
                () =>
                {
                    var timer = DateTime.Now;
                    PerfLogHelper.Attach("组1.开始");
                    Thread.Sleep(1000);
                    PerfLogHelper.Attach("组1.查酒店结束");

                    Thread.Sleep(2000);
                    PerfLogHelper.Attach("组1.查机票结束");

                    Thread.Sleep(100);
                    PerfLogHelper.Attach("组1.查可选项结束");

                },
                () =>
                {
                    var timer = DateTime.Now;
                    PerfLogHelper.Attach("组2.开始");
                    Thread.Sleep(1000);
                    PerfLogHelper.Attach("组2.查酒店结束");

                    Thread.Sleep(2000);
                    PerfLogHelper.Attach("组2.查机票结束");

                    Thread.Sleep(100);
                    PerfLogHelper.Attach("组2.查可选项结束");
                }
                );
            var actual = GetContextStr();

        }
    }

}
