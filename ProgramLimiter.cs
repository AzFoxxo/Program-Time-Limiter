namespace PTL
{
     struct ProgramLimiter
        {
            public string procName; // Process name
            public float time; // Time in minutes

            public ProgramLimiter(string procName, float time)
            {
                this.procName = procName;
                this.time = time;
            }
        }
}