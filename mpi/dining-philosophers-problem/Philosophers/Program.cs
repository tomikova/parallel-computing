using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPI;

namespace Philosophers
{
    class Program
    {
        public const string SPACING = "\t\t\t\t\t";

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Fork rightFork = null;
                Fork leftFork = null;
                bool rightToken = false;
                bool leftToken = false;
                int rightNeighbour = -1;
                int leftNeighbour = -1;
                string pom = "";

                Intracommunicator comm = Communicator.world;      
                int procesId = comm.Rank;
                rightNeighbour = procesId + 1;
                if (rightNeighbour > (comm.Size - 1))
                    rightNeighbour = 0;
                leftNeighbour = procesId - 1;
                if (leftNeighbour < 0)
                    leftNeighbour = comm.Size - 1;

                for (int i = 0; i < procesId; i++)
                    pom += SPACING;
                Console.WriteLine(pom + "Philosopher" + procesId.ToString());

                if (procesId == 0)
                {
                    rightFork = new Fork(true);
                    leftFork = new Fork(true);              
                }

                else if (procesId == (comm.Size - 1))
                {
                    rightFork = new Fork(false);
                    leftFork = new Fork(false);
                    rightToken = true;
                    leftToken = true;
                }

                else
                {
                    rightFork = new Fork(true);
                    leftFork = new Fork(false);
                    leftToken = true;
                }

                while (true)
                {
                    bool RightNbMessage = false;
                    bool LeftNbMessage = false;

                    Random rand = new Random();
                    int sleep = rand.Next(5);
                    Console.WriteLine(pom + "thinking");
                    for (int i = 0; i < sleep; i++)
                    {
                        RightNbMessage = CheckForMessage(rightNeighbour, comm);
                        LeftNbMessage = CheckForMessage(leftNeighbour, comm);

                        if (RightNbMessage)
                        {
                            comm.Receive<bool>(rightNeighbour, 0);
                            rightToken = true;
                        }

                        if (LeftNbMessage)
                        {
                            comm.Receive<bool>(leftNeighbour, 0);
                            leftToken = true;
                        }

                        if (rightToken)
                        {
                            MakeDecision(rightFork, rightNeighbour, comm);
                        }
                        if (leftToken)
                        {
                            MakeDecision(leftFork, leftNeighbour, comm);
                        }
                        System.Threading.Thread.Sleep(1000);
                    }

                    while (true)
                    {
                        RightNbMessage = false;
                        LeftNbMessage = false;

                        RightNbMessage = CheckForMessage(rightNeighbour, comm);
                        LeftNbMessage = CheckForMessage(leftNeighbour, comm);

                        if (RightNbMessage)
                        {
                            comm.Receive<bool>(rightNeighbour, 0);
                            rightToken = true;
                        }

                        if (LeftNbMessage)
                        {
                            comm.Receive<bool>(leftNeighbour, 0);
                            leftToken = true;
                        }

                        if (rightToken)
                        {
                            MakeDecision(rightFork, rightNeighbour, comm);
                        }
                        if (leftToken)
                        {
                            MakeDecision(leftFork, leftNeighbour, comm);
                        }

                        if (rightFork.haveFork && leftFork.haveFork)
                            break;

                        else
                        {
                            if (!rightFork.haveFork)
                            {
                                bool check = CheckForFork(rightNeighbour, comm);
                                if (check)
                                {
                                    Console.WriteLine(pom + "got fork" + rightNeighbour.ToString());
                                    comm.Receive<bool>(rightNeighbour, 1);
                                    rightFork.haveFork = true;
                                    rightFork.CLeanFork();
                                }
                            }
                            if (!rightFork.haveFork)
                            {
                                if (rightToken)
                                {
                                    AskForFork(rightNeighbour, comm);
                                    rightToken = false;
                                }
                            }

                            if (!leftFork.haveFork)
                            {
                                bool check = CheckForFork(leftNeighbour, comm);
                                if (check)
                                {
                                    Console.WriteLine(pom + "got fork" + leftNeighbour.ToString());
                                    comm.Receive<bool>(leftNeighbour, 1);
                                    leftFork.haveFork = true;
                                    leftFork.CLeanFork();
                                }
                            }
                            if (!leftFork.haveFork)
                            {
                                if (leftToken)
                                {
                                    AskForFork(leftNeighbour, comm);
                                    leftToken = false;
                                }
                            }
                        }
                    }

                    rand = new Random();
                    int eating = rand.Next(5001);
                    Console.WriteLine(pom + "eating");
                    rightFork.DirtyFork();
                    leftFork.DirtyFork();
                    System.Threading.Thread.Sleep(eating);

                    #region comm
                    RightNbMessage = false;
                    LeftNbMessage = false;

                    RightNbMessage = CheckForMessage(rightNeighbour, comm);
                    LeftNbMessage = CheckForMessage(leftNeighbour, comm);

                    if (RightNbMessage)
                    {
                        comm.Receive<bool>(rightNeighbour, 0);
                        rightToken = true;
                    }

                    if (LeftNbMessage)
                    {
                        comm.Receive<bool>(leftNeighbour, 0);
                        leftToken = true;
                    }

                    if (rightToken)
                    {
                        MakeDecision(rightFork, rightNeighbour, comm);
                    }
                    if (leftToken)
                    {
                        MakeDecision(leftFork, leftNeighbour, comm);
                    }
                    #endregion
                }
            }
        }

        static private void AskForFork(int neighbourId, Intracommunicator comm)
        {
            comm.Send<bool>(true, neighbourId, 0);
            string pom = "";
            for (int i = 0; i < comm.Rank; i++)
                pom += SPACING;
            Console.WriteLine(pom + "asking for fork" + neighbourId.ToString());
        }

        static private bool CheckForFork(int neighbourId, Intracommunicator comm)
        {
            var check = comm.ImmediateProbe(neighbourId, 1);
            if (check != null)
                return true;
            return false;
        }

        static private bool CheckForMessage(int neighbourId, Intracommunicator comm)
        {
            var check = comm.ImmediateProbe(neighbourId, 0);
            if (check != null)
                return true;
            return false;
        }

        static private void MakeDecision(Fork fork, int neighbourId, Intracommunicator comm)
        {
            if (fork.haveFork)
            {
                if (!fork.Dirty)
                    return;
                else if (fork.Dirty)
                {
                    string pom = "";
                    fork.haveFork = false;
                    for (int i = 0; i < comm.Rank; i++)
                        pom += SPACING;
                    Console.WriteLine(pom + "sending fork" + neighbourId.ToString());
                    comm.Send<bool>(true, neighbourId, 1);
                }
            }
        }
    }
}
