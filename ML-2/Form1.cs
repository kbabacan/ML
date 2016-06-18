

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


//Required Tables
//P  (RD      ) => RD
//P  (T |RD   ) => RD Given T
//P  (Z |T ,RD) => T and RD Given Z
//P  (RE|Z ,RD) => Z and RD Given RE
//P  (RI|RE,RD) => RE and RD Given RI


namespace ML_2
{
    public partial class Form1 : Form
    {
        const int NumberOfTehdit = 10;
        const int NumberOfZayiflik = 9;
        const int NumberOfRiskEtkisi = 5;
        const int NumberOfRiskIhtimali = 5;
        const int NumberOfRiskDuzeyi = 2;

        //PT Probability Table
        double[] PT_RD          = new double[NumberOfRiskDuzeyi];
        double[,] PT_T_RD       = new double[NumberOfTehdit,NumberOfRiskDuzeyi];
        double[, ,] PT_Z_T_RD   = new double[NumberOfZayiflik, NumberOfTehdit, NumberOfRiskDuzeyi];
        double[, ,] PT_RE_Z_RD  = new double[NumberOfRiskEtkisi, NumberOfZayiflik, NumberOfRiskDuzeyi];
        double[, ,] PT_RI_RE_RD = new double[NumberOfRiskIhtimali,NumberOfRiskEtkisi,NumberOfRiskDuzeyi];
       

        //Dependent Sums
        double DS_RD;
        double[]  DS_T  = new double[NumberOfRiskDuzeyi];
        double[,] DS_Z  = new double[NumberOfTehdit, NumberOfRiskDuzeyi],
                  DS_RE = new double[NumberOfZayiflik, NumberOfRiskDuzeyi],
                  DS_RI = new double[NumberOfRiskEtkisi, NumberOfRiskDuzeyi];


        //Performance Variables
        int TP=0, TN=0, FP=0, FN=0;
        double Accuracy;

        enum Tags {T=0,Z=1,RE=2,RI=3,RD=4};
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File | *.txt";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                button2.Enabled = true;
                StreamReader r = File.OpenText(ofd.FileName);
                string line = r.ReadLine();
                string[] entries = line.Split(',');
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                dataGridView1.Columns.Add("T",  entries[0]);
                dataGridView1.Columns.Add("Z",  entries[1]);
                dataGridView1.Columns.Add("RI", entries[2]);
                dataGridView1.Columns.Add("RE", entries[3]);
                dataGridView1.Columns.Add("RD", entries[4]);

                line = r.ReadLine();
                int count = 0;
                while(line!=null)
                {
                    entries = line.Split(',');
                    dataGridView1.Rows.Add(entries);
                    line = r.ReadLine();
                    count++;
                };
                MessageBox.Show(count + " data added to grid");

                /*
                 * 
                 *                                                      Done with Importing
                 * 
                 */

                for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
                {
                    int[] values = new int[5];
                    for (int j = 0; j < 5; j++)
                    {
                        string dummy = dataGridView1[j,i].Value.ToString(); //Transform data to integers to use as array indices
                        if(dummy.Length > 1) dummy = dummy.Remove(0, 1);
                        if (dummy.Equals("Y")) //Pozitif Grup - 1
                        {
                            values[j] = 1;
                            continue;
                        }
                        else if(dummy.Equals("A")) //Negatif Grup - 0
                        {
                            values[j] = 0;
                            continue;
                        }
                        values[j] = Int32.Parse(dummy) - 1;//Start from 0 
                    }
                    GenerateProbabilityTables(values);
                }
                UpdateProbabilityTables();
            }

        }

        private void UpdateProbabilityTables()
        {

            for (int i = 0; i < NumberOfRiskDuzeyi; i++)
            {
                Console.WriteLine(i + " " + PT_RD[i]);
            }



            //Calculate Table 2
            Console.WriteLine("Tehdit------------Risk Duzeyi");

            for (int i = 0; i < NumberOfTehdit; i++)
            {
                for (int j = 0; j < NumberOfRiskDuzeyi; j++)
                {
                    if (DS_T[j] > 0)
                    {
                        PT_T_RD[i, j] /= DS_T[j];
                    }
                    else
                    {
                        PT_T_RD[i, j] = 0;
                    }
                    Console.WriteLine(i + " " + j + " " + PT_T_RD[i, j] + " ");
                }
                //Console.WriteLine();
            }


            // Calculate Table 3
            Console.WriteLine("Zayiflik--------Tehdit--------Risk Duzeyi");

            for (int i = 0; i < NumberOfZayiflik; i++)
            {
                for (int j = 0; j < NumberOfTehdit; j++)
                {
                    for (int k = 0; k < NumberOfRiskDuzeyi; k++)
                    {
                        if (DS_Z[j, k] > 0)
                        {
                            PT_Z_T_RD[i, j, k] /= DS_Z[j, k];
                        }
                        else
                        {
                            PT_Z_T_RD[i, j, k] = 0;
                        }
                        Console.WriteLine(i + " " + j + " " + k + " | " + PT_Z_T_RD[i, j, k]);
                    }
                }
            }

            // Calculate Table 4
            Console.WriteLine("Risk Etkisi-----Zayiflik----------Risk Duzeyi");
            for (int i = 0; i < NumberOfRiskEtkisi; i++)
            {
                for (int j = 0; j < NumberOfZayiflik; j++)
                {
                    for (int k = 0; k < NumberOfRiskDuzeyi; k++)
                    {
                        if (DS_RE[j, k] > 0)
                        {
                            PT_RE_Z_RD[i, j, k] /= DS_RE[j, k];
                        }
                        else
                        {
                            PT_RE_Z_RD[i, j, k] = 0;
                        }
                        Console.WriteLine(i + " " + j + " " + k + " | " + PT_RE_Z_RD[i, j, k]);
                    }
                }
            }

            // Calculate Table 5
            Console.WriteLine("Risk Ihtimali-----Risk Etkisi--------Risk Duzeyi");
            for (int i = 0; i < NumberOfRiskIhtimali; i++)
            {
                for (int j = 0; j < NumberOfRiskEtkisi; j++)
                {
                    for (int k = 0; k < NumberOfRiskDuzeyi; k++)
                    {
                        if (DS_RI[j, k] > 0)
                        {
                            PT_RI_RE_RD[i, j, k] /= DS_RI[j, k];
                        }
                        else
                        {
                            PT_RI_RE_RD[i, j, k] = 0;
                        }
                        Console.WriteLine(i + " " + j + " " + k + " | " + PT_RI_RE_RD[i, j, k]);
                    }
                }
            }
        }

        void GenerateProbabilityTables(int[] values)
        {
            IntegerRep ir = new IntegerRep(values);
            PT_RD[ir.RD]++; //Equ. 1
            DS_RD++;

            PT_T_RD[ir.T, ir.RD]++; //Equ. 2
            DS_T[ir.RD]++;

            PT_Z_T_RD[ir.Z, ir.T, ir.RD]++; //Equ. 3
            DS_Z[ir.T, ir.RD]++;

            PT_RE_Z_RD[ir.RE, ir.Z, ir.RD]++; //Equ. 4
            DS_RE[ir.Z, ir.RD]++;

            PT_RI_RE_RD[ir.RI, ir.RE, ir.RD]++; //Equ. 5
            DS_RI[ir.RE, ir.RD]++;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File | *.txt";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
                StreamReader r = File.OpenText(ofd.FileName);
                string line = r.ReadLine();
                string[] entries = line.Split(',');
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                dataGridView1.Columns.Add("T", entries[0]);
                dataGridView1.Columns.Add("Z", entries[1]);
                dataGridView1.Columns.Add("RI", entries[2]);
                dataGridView1.Columns.Add("RE", entries[3]);
                dataGridView1.Columns.Add("RD", entries[4]);
                dataGridView1.Columns.Add("PR", "Probability");

                line = r.ReadLine();
                int count = 0;
                while (line != null)
                {
                    entries = line.Split(',');
                    dataGridView1.Rows.Add(entries);
                    line = r.ReadLine();
                    count++;
                };
                MessageBox.Show(count + " data added to grid");



                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    int[] values = new int[5];
                    for (int j = 0; j < 5; j++)
                    {
                        string dummy = dataGridView1[j, i].Value.ToString(); //Transform data to integers to use as array indices
                        if (dummy.Length > 1) dummy = dummy.Remove(0, 1);
                        if (dummy.Equals("Y")) //Pozitif Grup - 1
                        {
                            values[j] = 1;
                            continue;
                        }
                        else if (dummy.Equals("A")) //Negatif Grup - 0
                        {
                            values[j] = 0;
                            continue;
                        }
                        values[j] = Int32.Parse(dummy) - 1;//Start from 0 
                    }
                    double probability = CalculateProbability(values);
                    DataGridViewRow row = dataGridView1.Rows[i];
                    if (probability > 0.5) 
                    {
                        //row.DefaultCellStyle.BackColor = Color.Green;
                        Console.WriteLine("Y {0}",probability);
                        if (values[4] == 0)
                        {
                            FN++;
                        }
                        else
                        {
                            TP++;
                        }
                    }
                    else //0
                    {
                        //row.DefaultCellStyle.BackColor = Color.Red;
                        Console.WriteLine("A {0}", probability);
                        if (values[4] == 0)
                        {
                            TN++;
                        }
                        else
                        {
                            FP++;
                        }
                    }
                    row.Cells[row.Cells.Count - 1].Value = probability;
                }
            }
            Console.WriteLine(" True Pos {0} \n True Neg {1} \n False Pos {2} \n False Neg {3}",TP,TN,FP,FN);
            Accuracy = (double)(TP + TN) / (TP + TN + FP + FN);
            Console.WriteLine("Accuracy {0}",Accuracy);
        }

        double CalculateProbability(int[] values)
        {
            IntegerRep ir = new IntegerRep(values);
            double PleaseBeCorrect = PT_T_RD[ir.T, 1] * PT_Z_T_RD[ir.Z, ir.T, 1] * PT_RE_Z_RD[ir.RE, ir.Z, 1] * PT_RI_RE_RD[ir.RI, ir.RE, 1] * PT_RD[1];
            return PleaseBeCorrect / (PleaseBeCorrect + PT_T_RD[ir.T, 0] * PT_Z_T_RD[ir.Z, ir.T, 0] * PT_RE_Z_RD[ir.RE, ir.Z, 0] * PT_RI_RE_RD[ir.RI, ir.RE, 0] * PT_RD[0]);
        }   

        class IntegerRep
        {
            public int T;
            public int Z;
            public int RE;
            public int RI;
            public int RD;

            public IntegerRep(int[] vals)
            {
                T  = vals[0];
                Z  = vals[1];
                RE = vals[2];
                RI = vals[3];
                RD = vals[4];
            }
        }
    }

}
