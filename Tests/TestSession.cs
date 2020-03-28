using System;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;

namespace Tests
{
    public class TestSession : IDisposable
    {
        #region Types

        public class Error
        {
            #region Fields

            public readonly string Msg;

            public readonly Object Context;

            public readonly bool PrintContextObj;

            #endregion

            #region Constructors

            public Error(object msg, Object context = null, bool printContextObj = true)
            {
                Msg = msg == null ? "" : msg.ToString();
                PrintContextObj = printContextObj;
                Context = context;
            }

            #endregion

            #region Methods

            public void AppendToStringBuilder(StringBuilder stringBuilder)
            {
                stringBuilder.Append(
                    Context == null || !PrintContextObj
                        ? Msg
                        : $"[{Context}] {Msg}");
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly List<Error> _errors = new List<Error>();

        private readonly StringBuilder _errStringBuilder = new StringBuilder();

        #endregion

        #region Methods

        public void ReportErr(object msg, Object context = null, bool printContextObj = true)
        {
            _errors.Add(new Error(msg, context, printContextObj));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Assert();
        }

        private void Assert()
        {
            if (_errors.Count == 0)
            {
                return;
            }

            _errStringBuilder.Clear();
            foreach (var error in _errors)
            {
                error.AppendToStringBuilder(_errStringBuilder);
                _errStringBuilder.AppendLine();
            }

            var msg = _errStringBuilder.ToString().Trim();
            NUnit.Framework.Assert.Fail(msg);
        }

        #endregion
    }
}