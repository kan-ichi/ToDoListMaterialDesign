using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoListMaterialDesign.Models.Interface
{
    /// <summary>
    /// 処理応答
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// 処理成功の応答
        /// </summary>
        bool IsSucceed { get; set; }

        /// <summary>
        /// 処理エラーの場合、その内容を応答
        /// </summary>
        System.Collections.IList Errors { get; set; }

        /// <summary>
        /// 処理中に警告が発生した場合、その内容を応答
        /// </summary>
        System.Collections.IList Warnings { get; set; }

        /// <summary>
        /// 処理結果を返却
        /// </summary>
        object Result { get; set; }

        /// <summary>
        /// 処理結果メッセージを返却
        /// </summary>
        string Message { get; set; }
    }
}
