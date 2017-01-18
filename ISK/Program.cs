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
            FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"../../Results/", "population3.txt");

            const int populationSize = 50;

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
            var elite = new Elite(5);

            //create the crossover operator
            //var crossover = new Crossover(1)
            //{
            //    CrossoverType = CrossoverType.DoublePointOrdered
            //};

            var crossover = new CustomCrossoverOperator()
            {
                CrossoverType = CustomCrossoverType.MX1
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
            ga.Operators.Add(mutate);

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

        private static void connectNode(GraphNode node, Dictionary<int, GraphNode> connected)
        {
            foreach(GraphNode neighbour in node.neighbours)
            {
                if (!connected.ContainsKey(neighbour.Id))
                {
                    connected[neighbour.Id] = neighbour;
                    connectNode(neighbour,connected);
                }
            }
        }
        
        private static IEnumerable<GraphNode> CreateNodes()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"../../Graphs/Graph_100_10(20,30,20,30).txt");

            // Skip first line
            var lines = File.ReadLines(path).Skip(1).ToList();
            var nodes = new Dictionary<int, GraphNode>();
            var connected = new Dictionary<int, GraphNode>();
            
            foreach (var line in lines)
            {
                var node1Index = int.Parse(line.Split(null)[1]);
                var node2Index = int.Parse(line.Split(null)[2]);

                GraphNode node1, node2;

                var result1 = nodes.TryGetValue(node1Index, out node1);
                if(!result1)
                {
                    nodes[node1Index] = node1 = new GraphNode(node1Index, node1Index == 0);
                    if(node1Index == 0)
                    {
                        connected[node1Index] = node1;
                    }
                }

                var result2 = nodes.TryGetValue(node2Index, out node2);
                if (!result2)
                {
                    nodes[node2Index] = node2 = new GraphNode(node2Index, node2Index == 0);
                    if (node2Index == 0)
                    {
                        connected[node1Index] = node1;
                    }
                }
                if (connected.ContainsKey(node1Index) && !connected.ContainsKey(node2Index)) {
                    connected[node2Index] = node2;
                    connectNode(node2, connected);
                }
                if (connected.ContainsKey(node2Index) && !connected.ContainsKey(node1Index)) {
                    connected[node1Index] = node1;
                    connectNode(node1, connected);
                }

                node1.addNeighbour(node2);
            }
            foreach (int key in nodes.Keys)
            {
                if (!connected.ContainsKey(key))
                {
                    Console.WriteLine("NONCONNECTED {0}",key);
                }
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
                withoutMessage.Reverse();
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
                withoutMessage.Reverse();
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
