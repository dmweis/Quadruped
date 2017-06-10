using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using DynamixelServo.Driver;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;

namespace DynamixelServo.WPFMonitor
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public ObservableCollection<Servo> Servos { get; } = new ObservableCollection<Servo>();
      private DynamixelDriver _driver;

      public MainWindow()
      {
         InitializeComponent();
         DataContext = this;
         _driver = new DynamixelDriver("COM17");
         foreach (var index in _driver.Search(1, 10))
         {
            Servo newServo = new Servo() {Index = index};
            newServo.ToggleLed += (sender, servo) =>
            {
               _driver.SetLed((byte)servo.Index, servo.LedOn);
            };
            Servos.Add(newServo);
         }
         foreach (var servo in Servos)
         {
            _driver.SetTorque((byte)servo.Index, false);
         }
         Timer timer = new Timer();
         timer.AutoReset = true;
         timer.Elapsed += (sender, args) =>
         {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
               foreach (var servo in Servos)
               {
                  servo.Temperature = _driver.ReadTemperature((byte) servo.Index);
                  servo.Position = _driver.ReadPresentPosition((byte) servo.Index);
               }
            });
         };
         timer.Interval = 100;
         timer.Start();
      }
   }

   public class Servo : ViewModelBase
   {
      public event EventHandler<Servo> ToggleLed;
      public bool LedOn;
      public RelayCommand ToggleLedCommand { get; }

      private int _temperature;
      public int Temperature
      {
         get => _temperature;
         set => Set(ref _temperature, value);
      }

      private int _index;
      public int Index
      {
         get => _index;
         set => Set( ref _index, value);
      }

      private int _position;
      public int Position
      {
         get => _position;
         set => Set( ref _position , value);
      }

      public Servo()
      {
         ToggleLedCommand = new RelayCommand(() =>
         {
            LedOn = !LedOn;
            ToggleLed?.Invoke(this, this);
         });
      }
   }
}
