namespace LOVE.NET.Common
{
    public class Result
    {
        public bool Succeeded { get; private set; }

        public bool Failure => !this.Succeeded;

        public string[] Errors { get; private set; }

        public static implicit operator Result(bool succeeded)
            => new Result { Succeeded = succeeded };

        public static implicit operator Result(string error)
            => new Result
            {
                Succeeded = false,
                Errors = new[] { error },
            };
    }
}
