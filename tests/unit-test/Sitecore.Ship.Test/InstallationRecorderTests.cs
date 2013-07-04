﻿using System;
using System.Collections.Generic;
using Moq;
using Sitecore.Ship.Core.Contracts;
using Sitecore.Ship.Core.Domain;
using Sitecore.Ship.Infrastructure.Update;
using Xunit;

namespace Sitecore.Ship.Test
{
    public class InstallationRecorderTests
    {
        private Mock<IPackageHistoryRepository> _mockPackageHistoryRepository;
        private Mock<IConfigurationProvider<PackageInstallationSettings>> _mockConfigurationProvider;

        public InstallationRecorderTests()
        {
            _mockPackageHistoryRepository = new Mock<IPackageHistoryRepository>();
            _mockConfigurationProvider = new Mock<IConfigurationProvider<PackageInstallationSettings>>();
            _mockConfigurationProvider.Setup(x => x.Settings.RecordInstallationHistory).Returns(true);
        }

        [Fact]
        public void recordinstall_does_not_install_when_disabled()
        {
            _mockConfigurationProvider.Setup(x => x.Settings.RecordInstallationHistory).Returns(false);
            var recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _mockConfigurationProvider.Object);
            var dateInstalled = DateTime.Now;

            recorder.RecordInstall("01-Description.zip", dateInstalled);

            _mockPackageHistoryRepository.Verify(x => x.Add(It.IsAny<InstalledPackage>()), Times.Never());
        }

        
        [Fact]
        public void recordinstall_parses_packageid_correctly_when_relative_path_provided()
        {
            var recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _mockConfigurationProvider.Object);
            var dateInstalled = DateTime.Now;

            recorder.RecordInstall("01-Description.zip", dateInstalled);

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.PackageId == "01")));
        }

        [Fact]
        public void recordinstall_parses_packageid_correctly_when_full_path_provided()
        {
            var recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _mockConfigurationProvider.Object);
            var dateInstalled = DateTime.Now;

            recorder.RecordInstall("C:\\aaa\\bbb\\01-Description.zip", dateInstalled);

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.PackageId == "01")));
        }

        [Fact]
        public void recordinstall_parses_description_correctly()
        {
            var recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _mockConfigurationProvider.Object);
            var dateInstalled = DateTime.Now;

            recorder.RecordInstall("01-Description.zip", dateInstalled);

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.Description == "Description")));
        }

        [Fact]
        public void getlatest_returns_highest_version_when_multipl_entries()
        {
            _mockPackageHistoryRepository.Setup(x => x.GetAll()).Returns(
                new List<InstalledPackage>()
                    {
                        new InstalledPackage() {PackageId = "01"},
                        new InstalledPackage() {PackageId = "02"},
                        new InstalledPackage() {PackageId = "03"},
                        
                    });

            var recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _mockConfigurationProvider.Object);

            var expected = new InstalledPackage() {PackageId = "03"};
            var result = recorder.GetLatestPackage();

            Assert.Same(result.PackageId, expected.PackageId);
        }
    }
}
