using System;
using ToDoListMaterialDesign.Models.Interface;

namespace ToDoListMaterialDesign.Models.Logics
{
    /// <summary>
    /// 処理依頼
    /// </summary>
    [Serializable]
    public class Request : IRequest
    {
        /// <summary>
        /// 処理依頼パラメーター
        /// </summary>
        public object Parameter { get; set; }
    }
}
