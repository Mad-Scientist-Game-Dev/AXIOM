using Axiom.Arbiter;

class Program
{
    static async Task Main(string[] args)
    {
        var arbiter = new ArbiterHost();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            await arbiter.HandleHumanInputAsync(input);
        }
    }
}
