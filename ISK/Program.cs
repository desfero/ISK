using System;
using System.Collections.Generic;
using System.Linq;
using GAF;
using GAF.Extensions;
using GAF.Operators;
using System.IO;
using System.Reflection;

namespace ISK
{
    internal class Program
    {
        static string FilePath { get; set; }

        private static void Main(string[] args)
        {
            FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"../../Results/", "test5.txt");

            const int populationSize = 100;

            //get our nodes
            var nodes = CreateNodes();

            var population = new Population();

            //create the chromosomes
            for (var p = 0; p < populationSize; p++)
            {

                var chromosome = new Chromosome();
                foreach (var node in nodes)
                {
                    chromosome.Genes.Add(new Gene(node));
                }

                var rnd = GAF.Threading.RandomProvider.GetThreadRandom();
                chromosome.Genes.ShuffleFast(rnd);

                population.Solutions.Add(chromosome);
            }

            //create the elite operator
            var elite = new Elite(3);

            //create the crossover operator
            //var crossover = new Crossover(0.8)
            //{
            //    CrossoverType = CrossoverType.DoublePointOrdered
            //};

            var crossover = new CustomCrossoverOperator()
            {
                CrossoverType = CustomCrossoverType.PMX
            };
            //create the mutation operator
            var mutate = new SwapMutate(0.05);

            //create the GA
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //hook up to some useful events
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            //add the operators
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            //ga.Operators.Add(mutate);

            //run the GA
            ga.Run(Terminate);

            Console.Read();
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Write(((GraphNode)gene.ObjectValue).Id);
            }
            Write("SendingSequence:");
            printoutSequenceOfMessages(fittest);
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            Write(String.Format("Generation: {0}, Fitness: {1}", e.Generation, fittest.Fitness));
        }

        private static IEnumerable<GraphNode> CreateNodes()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"../../Graphs/graf.txt");

            // Skip first line
            var lines = File.ReadLines(path).Skip(1).ToList();
            var nodes = new Dictionary<int, GraphNode>();

            foreach (var line in lines)
            {
                var node1Index = int.Parse(line.Split('\t')[1]);
                var node2Index = int.Parse(line.Split('\t')[2]);

                GraphNode node1, node2;

                var result1 = nodes.TryGetValue(node1Index, out node1);
                if(!result1)
                {
                    nodes[node1Index] = node1 = new GraphNode(node1Index, node1Index == 1);
                }

                var result2 = nodes.TryGetValue(node2Index, out node2);
                if (!result2)
                {
                    nodes[node2Index] = node2 = new GraphNode(node2Index, node2Index == 1);
                }

                node1.addNeighbour(node2);
            }

            return nodes.Values;
        }

        public static void printoutSequenceOfMessages(Chromosome chromosome)
        {
            List<GraphNode> nodes = chromosome.Genes.Select<Gene, GraphNode>((g) => ((GraphNode)g.ObjectValue)).ToList();
            List<GraphNode> withMessage = new List<GraphNode>();
            List<GraphNode> withoutMessage = new List<GraphNode>();
            foreach (GraphNode node in nodes)
            {
                if (node.StartNode)
                {
                    withMessage.Add(node);
                }
                else
                {
                    withoutMessage.Add(node);
                }
            }

            var rounds = 0;
            while (withoutMessage.Count > 0)
            {
                List<GraphNode> receiving = new List<GraphNode>();
                rounds++;
                withoutMessage.Sort();
                Write(String.Format("Round {0}, fight!", rounds));
                foreach (GraphNode sender in withMessage)
                {
                    foreach (GraphNode receiver in withoutMessage)
                    {
                        if (sender.isNeighbour(receiver))
                        {
                            receiving.Add(receiver);
                            withoutMessage.Remove(receiver);
                            Write(String.Format("{0} sends to {1},", sender.Id, receiver.Id));
                            break;
                        }
                    }
                }
                withMessage = withMessage.Concat(receiving).ToList();
            }
            
        }

        public static void Write(object line)
        {
            Console.WriteLine(line);
            File.AppendAllLines(FilePath, new[] { line.ToString() });
        }

        public static double CalculateFitness(Chromosome chromosome)
        {

            List<GraphNode> nodes = chromosome.Genes.Select<Gene, GraphNode>((g) => ((GraphNode)g.ObjectValue)).ToList();
            List<GraphNode> withMessage = new List<GraphNode>();
            List<GraphNode> withoutMessage = new List<GraphNode>();
            foreach(GraphNode node in nodes)
            {
                if (node.StartNode)
                {
                    withMessage.Add(node);
                }
                else
                {
                    withoutMessage.Add(node);
                }
            }
            var rounds = 0;
            while (withoutMessage.Count > 0)
            {
                List<GraphNode> receiving = new List<GraphNode>();
                rounds++;
                withoutMessage.Sort();
                foreach (GraphNode sender in withMessage)
                {
                    foreach (GraphNode receiver in withoutMessage)
                    {
                        if (sender.isNeighbour(receiver))
                        {
                            receiving.Add(receiver);
                            withoutMessage.Remove(receiver);
                            break;
                        }
                    }
                }
                withMessage = withMessage.Concat(receiving).ToList();
            }
            return (rounds>0)?((double)1/rounds):1; //fitness value must be between 0 and 1, so here max fitness is 1 when doing everything in one round, getting lower with more rounds.
        }

        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 100;
        }
    }
}
