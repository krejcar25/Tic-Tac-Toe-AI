﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tic_Tac_Toe_AI
{
    /// <summary>
    /// Interaction logic for NNView.xaml
    /// </summary>
    public partial class NNView : Window
    {
        public NNView(NeuralNetwork neuralNetwork)
        {
            InitializeComponent();
        }
    }
}
