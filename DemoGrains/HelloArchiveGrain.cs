﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using SharedOrleansInterface;

namespace DemoGrains
{
    public class HelloArchiveGrain : Grain<GreetingArchive>, IHelloArchive
    {
        public async Task<string> SayHello(string greeting)
        {
            State.Greetings.Add(greeting);

            await WriteStateAsync();

            return $"You said: '{greeting}', I say: Hello!";
        }

        public Task<IEnumerable<string>> GetGreetings()
        {
            return Task.FromResult<IEnumerable<string>>(State.Greetings);
        }
    }

    public class GreetingArchive
    {
        public List<string> Greetings { get; } = new List<string>();
    }
}