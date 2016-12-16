﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAF;

namespace ISK
{
    public class CustomCrossoverOperator : IGeneticOperator
    {
        
        private int replacementRate;

        private bool enabled;

        private CustomCrossoverType crossoverType;
        
        
        public void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnesFunctionDelegate)
        {
            int eliteCount = 0;
            if (currentPopulation.Solutions == null || currentPopulation.Solutions.Count == 0)
            {
                throw new ArgumentException("There are no Solutions in the current Population.");
            }

            if (newPopulation == null)
            {
                newPopulation = currentPopulation.CreateEmptyCopy();
            }
            var elites = currentPopulation.GetElites();
            eliteCount = elites.Count();
            if (elites != null && eliteCount > 0)
            {
                newPopulation.Solutions.AddRange(elites);
            }

            int childAmount = (currentPopulation.PopulationSize - newPopulation.PopulationSize);
            for (var i = 0; i < childAmount; i++){
                newPopulation.Solutions.Add(generateChild(currentPopulation));
            }
        }

        public int GetOperatorInvokedEvaluations()
        {
            return 0;
        }

        private Chromosome generateChild(Population potentialParents)
        {
            var parents = potentialParents.SelectParents();
            var firstGeneSet = new List<Gene>(parents[0].Genes);
            var secondGeneSet = new List<Gene>(parents[1].Genes);
            var child = new Chromosome();
            int geneCount = potentialParents.ChromosomeLength;

            switch (crossoverType)
            {
                case CustomCrossoverType.MX1:
                    getChildMX1(child, firstGeneSet, secondGeneSet, geneCount);
                    break;
                case CustomCrossoverType.MX2:
                    getChildMX2(child, firstGeneSet, secondGeneSet, geneCount);
                    break;
                case CustomCrossoverType.CX:
                    
                    break;
                case CustomCrossoverType.PMX:
                    getChildPMX(child, firstGeneSet, secondGeneSet, geneCount);
                    break;
            }
            
            return child;
        }

        private void getChildMX1(Chromosome child, List<Gene> firstGeneSet, List<Gene> secondGeneSet, int geneCount)
        {
            for (var i = 0; i < geneCount; i++)
            {
                var firstNode = (GraphNode)firstGeneSet[i].ObjectValue;
                var secondNode = (GraphNode)secondGeneSet[i].ObjectValue;
                if (firstNode.neighbours.Count > secondNode.neighbours.Count)
                {
                    addToChildAndSwap(child, secondGeneSet, secondNode, firstGeneSet, i);
                }
                else
                {
                    addToChildAndSwap(child, firstGeneSet, firstNode, secondGeneSet, i);
                }
            }
        }

        private void addToChildAndSwap(Chromosome child, List<Gene> chosenGeneSet, GraphNode chosenNode, List<Gene> notChosenGeneSet, int choiceIndex)
        {
            child.Add(chosenGeneSet[choiceIndex]);
            var swapIndex = notChosenGeneSet.FindIndex((g) => ((GraphNode)g.ObjectValue).GetHashCode() == chosenNode.GetHashCode());
            swapElements(notChosenGeneSet, choiceIndex, swapIndex);
        }

        private void getChildMX2(Chromosome child, List<Gene> firstGeneSet, List<Gene> secondGeneSet, int geneCount)
        {
            for (var i = 0; i < geneCount; i++)
            {
                var firstNode = firstGeneSet.Count > 0?(GraphNode)firstGeneSet[0].ObjectValue:null;
                var secondNode = secondGeneSet.Count>0?(GraphNode)secondGeneSet[0].ObjectValue:null;
                if (secondNode != null &&(firstNode == null || firstNode.neighbours.Count > secondNode.neighbours.Count))
                {
                    addToChildAndRemoveFromParents(child, secondGeneSet, secondNode, firstGeneSet);
                }
                else
                {
                    addToChildAndRemoveFromParents(child, firstGeneSet, firstNode, secondGeneSet);
                }
            }
        }

        private void addToChildAndRemoveFromParents(Chromosome child, List<Gene> chosenGeneSet, GraphNode chosenNode, List<Gene> notChosenGeneSet)
        {
            child.Add(chosenGeneSet[0]);
            chosenGeneSet.RemoveAt(0);
            var removeIndex = notChosenGeneSet.FindIndex((g) => ((GraphNode)g.ObjectValue).GetHashCode() == chosenNode.GetHashCode());
            notChosenGeneSet.RemoveAt(removeIndex);
        }

        private void getChildPMX(Chromosome child, List<Gene> firstGeneSet, List<Gene> secondGeneSet, int geneCount)
        {
            for (var i = 0; i < geneCount; i++)
            {
                var firstNode = (GraphNode)firstGeneSet[i].ObjectValue;
                var secondNode = (GraphNode)secondGeneSet[i].ObjectValue;
                if (firstNode.neighbours.Count > secondNode.neighbours.Count)
                {
                    addToChildAndSwap(child, secondGeneSet, secondNode, firstGeneSet, i);
                }
                else
                {
                    addToChildAndSwap(child, firstGeneSet, firstNode, secondGeneSet, i);
                }
            }
        }

        private void swapElements (List<Gene> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public bool Enabled { get { return enabled; } set { enabled = value; } }

        public CustomCrossoverType CrossoverType
        {
            get
            {
                return crossoverType;
            }

            set
            {
                crossoverType = value;
                enabled = true;
            }
        }
    }
}
