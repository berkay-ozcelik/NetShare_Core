using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetShare_Core.Entity
{
    public class CommandResult
    {

        public enum ResultType
        {
            Success,
            Error
        }

        public ResultType Type { get; }
        public string Message { get; }

        private CommandResult(ResultType type, string message)
        {
            Type = type;
            Message = message;
        }


        public static CommandResult Success(string message)
        {
            return new CommandResult(ResultType.Success, message);
        }

        public static CommandResult Success()
        {
            return new CommandResult(ResultType.Success, string.Empty);
        }

        public static CommandResult Error(string message)
        {
            return new CommandResult(ResultType.Error, message);
        }

        public static CommandResult Error()
        {
            return new CommandResult(ResultType.Error, string.Empty);
        }

    }
}
