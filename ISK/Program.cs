using System;
using System.Collections.Generic;
using System.Linq;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace ISK
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const int populationSize = 100;

            //get our cities
            var nodes = CreateNodes().ToList();

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
            var crossover = new Crossover(0.8)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            //create the mutation operator
            var mutate = new SwapMutate(0.02);

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
                Console.WriteLine(((GraphNode)gene.ObjectValue).Id);
            }
            Console.WriteLine("SendingSequence:");
            printoutSequenceOfMessages(fittest);
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            Console.WriteLine("Generation: {0}, Fitness: {1}", e.Generation, fittest.Fitness);
        }

        private static IEnumerable<GraphNode> CreateNodes()
        {
            var nodes = new List<GraphNode>();
            var n1 = new GraphNode(1, true);
            nodes.Add(n1);
            var n2 = new GraphNode(2, false);
            nodes.Add(n2);
            var n3 = new GraphNode(3, false);
            nodes.Add(n3);
            var n4 = new GraphNode(4, false);
            nodes.Add(n4);
            var n5 = new GraphNode(5, false);
            nodes.Add(n5);
            var n6 = new GraphNode(6, false);
            nodes.Add(n6);
            var n7 = new GraphNode(7, false);
            nodes.Add(n7);
            var n8 = new GraphNode(8, false);
            nodes.Add(n8);
            var n9 = new GraphNode(9, false);
            nodes.Add(n9);

            var n10 = new GraphNode(10, false);
            nodes.Add(n10);

            var n11 = new GraphNode(11, false);
            nodes.Add(n11);

            var n12 = new GraphNode(12, false);
            nodes.Add(n12);

            var n13 = new GraphNode(13, false);
            nodes.Add(n13);

            var n14 = new GraphNode(14, false);
            nodes.Add(n14);

            var n15 = new GraphNode(15, false);
            nodes.Add(n15);

            var n16 = new GraphNode(16, false);
            nodes.Add(n16);

            var n17 = new GraphNode(17, false);
            nodes.Add(n17);

            var n18 = new GraphNode(18, false);
            nodes.Add(n18);

            n1.addNeighbour(n2);
            n1.addNeighbour(n3);
            

            n2.addNeighbour(n5);
            n2.addNeighbour(n4);
            n2.addNeighbour(n11);

            n3.addNeighbour(n6);
            n3.addNeighbour(n7);
            n3.addNeighbour(n14);

            n4.addNeighbour(n5);
            n4.addNeighbour(n6);
            n4.addNeighbour(n8);
            n4.addNeighbour(n16);
            n4.addNeighbour(n17);

            n6.addNeighbour(n7);
            n6.addNeighbour(n9);
            n6.addNeighbour(n17);
            n6.addNeighbour(n18);



            n8.addNeighbour(n9);

            n8.addNeighbour(n13);

            n9.addNeighbour(n4);

            n10.addNeighbour(n11);
            n10.addNeighbour(n13);

            n11.addNeighbour(n12);

            n15.addNeighbour(n17);
            n15.addNeighbour(n18);

            return nodes;
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
                withoutMessage.Reverse(); ///// ??? got confused, not sure if needed
                Console.WriteLine("Round {0}, fight!", rounds);
                foreach (GraphNode sender in withMessage)
                {
                    foreach (GraphNode receiver in withoutMessage)
                    {
                        if (sender.isNeighbour(receiver))
                        {
                            receiving.Add(receiver);
                            withoutMessage.Remove(receiver);
                            Console.WriteLine("{0} sends to {1},", sender.Id, receiver.Id);
                            break;
                        }
                    }
                }
                withMessage = withMessage.Concat(receiving).ToList();
            }
            
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
                withoutMessage.Reverse(); ///// ??? got confused, not sure if needed
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
            return currentGeneration > 400;
        }

    }
}
