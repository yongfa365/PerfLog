using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PerfLog.Core
{
    public class PerfLogNode
    {
        /// <summary>
        /// 要记录的内容的说明，不能是中文，因为是放在Header里的，转乱会浪费时间
        /// </summary>
        [JsonProperty("N")]
        public string Name { get; set; }

        /// <summary>
        /// 消耗的时间
        /// </summary>
        [JsonProperty("V")]
        public int Value { get; set; }

        /// <summary>
        /// 子级，细节日志
        /// </summary>
        [JsonProperty("C")]
        public List<PerfLogNode> Children { get; set; }
    }

}
