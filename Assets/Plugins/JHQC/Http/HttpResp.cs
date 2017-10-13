using UnityEngine;
using System.Collections;

namespace Jhqc.EditorCommon
{
    /// <summary>
    /// HttpResp
    /// </summary>
    public class HttpResp
    {
        public string WwwText { get; set; }
        public ErrorType Error { get; set; }
        internal string ErrorText { get; set; }
        internal WWW Raw { get; set; }

        /// <summary>
        /// 在Error的时候，会tostring成error内容
        /// </summary>
        public override string ToString()
        {
            if (Error != ErrorType.None)
            {
                return Error.ToString() + "   " + ErrorText;
            }
            else
            {
                return WwwText;
            }
        }

        public enum ErrorType
        {
            None,
            NetworkError,
            AccessExpired,
            Timeout,
            LogicError,
            JsonError
        }
    }
}