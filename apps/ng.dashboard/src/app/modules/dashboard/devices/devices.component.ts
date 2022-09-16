import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {DeviceService} from '@app/services/device.service';
import {DeviceDto} from '@app/models/dtos';
import {RegisterDeviceDialogComponent} from '@app/components/dialogs/register-device/register-device.component';
import {EditDeviceDialogComponent} from '@app/components/dialogs/edit-device/edit-device.component';
import {TableData} from '@app/models/table-data';
import {Observable} from 'rxjs/Observable';

@Component({
  selector: 'app-devices',
  templateUrl: './devices.component.html',
  styleUrls: ['./devices.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class DevicesComponent implements OnInit {
  keywords = '';
  tableData: TableData;

  constructor(private deviceService: DeviceService, public dialog: MatDialog, public snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.loadDevices();
  }

  search(key: any): void {

  }

  addNew(): void {
    const dto = new DeviceDto(
      {
        serialNumber: '',
        deviceGroupName: '',
        organizationName: '',
        assetTag: '',
        deviceVersion: null,
        appVersion: null,
        locationId: '',
        activatedOn: null
      }
    );
    const dialogRef = this.dialog.open(RegisterDeviceDialogComponent, {
      width: '480px',
      data: dto,
      disableClose: true,
      hasBackdrop: true
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        this.deviceService.registerDevice(data).subscribe(results => {
            this.loadDevices();
          },
          error => {
            this.snackBar.open('Failed to register new device ', 'FAILURE', {
              duration: 5000,
              panelClass: ['action-error']
            });
          });
      }
    });
  }

  loadDevices(): void {
    this.deviceService.getDevices().subscribe(results => {
      const devices = results.map(v => {
        v.checked = false;
        return v;
      });

      this.tableData = {
        title: 'All Devices',
        template: 'deviceTemplate',
        headerRow: ['Group', 'Serial Number', 'Asset Tag', 'License'],
        dataRows: devices
      };
    });
  }

  toggle(video): void {
    video.checked = !video.checked;
  }

  edit(serial: string): void {
    this.deviceService.getDevice(serial).subscribe(dto => {
      const dialogRef = this.dialog.open(EditDeviceDialogComponent, {
        data: dto
      });

      dialogRef.afterClosed().subscribe(data => {
        if (data && data.id) {
          for (let i = 0; i < this.tableData.dataRows.length; i++) {
            if (this.tableData.dataRows[i].id === data.id) {
              this.tableData.dataRows[i] = data;
            }
          }
        }
      });
    });
  }

  remove(dto: DeviceDto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      data: {title: dto.serialNumber, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        this.deviceService.deleteDevice(dto.id).subscribe(result => {
          // refresh the result
          this.loadDevices();
          this.snackBar.open('device with serial number \''
            + dto.serialNumber + '\' has been successfully removed.',
            'SUCCESS', {
              duration: 3000,
              panelClass: ['action-success']
            });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove device with serial umber \''
            + dto.serialNumber + '\'', 'FAILURE', {
            duration: 5000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }
}
