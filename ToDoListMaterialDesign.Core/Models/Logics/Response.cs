using System;
using System.Collections;
using System.Collections.Generic;
using ToDoListMaterialDesign.Models.Interface;

namespace ToDoListMaterialDesign.Models.Logics
{
    /// <summary>
    /// 処理応答
    /// </summary>
    [Serializable]
    public class Response : IResponse
    {
        /// <summary>
        /// 処理成功の応答
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// 処理エラーの場合、その内容を応答
        /// </summary>
        public IList Errors { get; set; } = new List<Error>();

        /// <summary>
        /// 処理中に警告が発生した場合、その内容を応答
        /// </summary>
        public IList Warnings { get; set; } = new List<Warning>();

        /// <summary>
        /// 処理結果を返却
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 処理結果メッセージを返却
        /// </summary>
        public string Message { get; set; } = null;
    }
}
