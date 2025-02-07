﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using EasyLicense.Lib;
using EasyLicense.Lib.License;
using EasyLicense.Lib.License.Validator;

namespace EasyLicense.LicenseTool {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.txtName.Text = "CoordTransform";
            this.limitTimePicker.SelectedDate = DateTime.UtcNow.AddDays(10);
            txtComputerKey.Text = new HardwareInfo().GetHardwareString();
        }

        private void btnGenerateLicense_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtComputerKey.Text)) {
                MessageBox.Show("Some field is missing");
                return;
            }
            if (limitTimePicker.SelectedDate is null) {
                MessageBox.Show("limit time cant be none");
                return;
            }

            string softName = this.txtName.Text;
            string computerKey = this.txtComputerKey.Text;
            DateTime limitTime = this.limitTimePicker.DisplayDate;


            GenerateLicense(softName, computerKey, limitTime);

            ValidateLicense();
        }

        private void GenerateLicense(string softName, string computerKey, DateTime limitTime) {


            if (!File.Exists("privateKey.xml")) {
                MessageBox.Show("Please create a license key first");
                return;
            }



            var privateKey = File.ReadAllText(@"privateKey.xml");
            var generator = new LicenseGenerator(privateKey);

            var dict = new Dictionary<string, string>();

            dict["name"] = softName;
            dict["key"] = computerKey;

            // generate the license
            var license = generator.Generate("EasyLicense", Guid.NewGuid(), limitTime, dict,
                LicenseType.Standard);

            txtLicense.Text = license;
            File.WriteAllText("license.lic", license);

            File.AppendAllText("license.log", $"License to {dict["name"]}, key is {dict["key"]}, Date is {DateTime.Now}");
        }

        private void ValidateLicense() {
            if (!File.Exists("publicKey.xml")) {
                MessageBox.Show("Please create a license key first");
                return;
            }

            var publicKey = File.ReadAllText(@"publicKey.xml");

            var validator = new LicenseValidator(publicKey, @"license.lic");

            try {
                validator.AssertValidLicense();

                var dict = validator.LicenseAttributes;
                MessageBox.Show($"License to {dict["name"]}, key is {dict["key"]}");

                if (dict["key"] != txtComputerKey.Text) {
                    MessageBox.Show("invalid!");
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private void btnGenerateLicenseKey_Click(object sender, RoutedEventArgs e) {
            // var assembly = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists("privateKey.xml") || File.Exists("publicKey.xml")) {
                var result = MessageBox.Show("The key is existed, override it?", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) {
                    return;
                }
            }

            var privateKey = "";
            var publicKey = "";
            LicenseGenerator.GenerateLicenseKey(out privateKey, out publicKey);

            File.WriteAllText("privateKey.xml", privateKey);
            File.WriteAllText("publicKey.xml", publicKey);

            MessageBox.Show("The Key is created, please backup it.");
        }
    }
}