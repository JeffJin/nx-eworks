import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {CustomerDto, DeviceDto, GroupDto, LocationDto} from '@app/models/dtos';
import {DeviceService} from '@app/services/device.service';
import {GroupService} from '@app/services/group.service';
import {CustomerService} from '@app/services/customer.service';
import {LocationService} from '@app/services/location.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-edit-device-dialog',
  templateUrl: './edit-device.component.html',
  styleUrls: ['./edit-device.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditDeviceDialogComponent implements OnInit {
  selectedGroup: GroupDto;
  selectedCustomer: CustomerDto;
  selectedLocation: LocationDto;
  groups: any[];
  customers: any[];
  locations: any[];

  constructor(private deviceService: DeviceService, private groupService: GroupService, private locationService: LocationService,
              private customerService: CustomerService, public dialogRef: MatDialogRef<EditDeviceDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) {
  }

  ngOnInit(): void {
    this.loadGroups();
    this.loadCustomers();
    this.loadLocations();
  }

  loadCustomers() {
    this.customerService.getCustomers().subscribe(results => {
      this.customers = results;
      this.selectedCustomer = this.customers.find(c => c.name === this.data.organizationName);
    });
  }

  loadGroups() {
    this.groupService.loadGroups().subscribe(results => {
      this.groups = results;
      this.selectedGroup = this.groups.find(c => c.name === this.data.deviceGroupName);
    });
  }

  loadLocations() {
    this.locationService.loadLocations().subscribe(results => {
      this.locations = results;
      this.selectedLocation = this.locations.find(c => c.id === this.data.locationId);
    });
  }

  localeSelected(dto: any) {

  }

  groupSelected(dto: any) {

  }

  customerSelected(dto: any) {

  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(dto: DeviceDto) {
    dto.deviceGroupName = this.selectedGroup ? this.selectedGroup.name : '';
    dto.organizationName = this.selectedCustomer ? this.selectedCustomer.name : '';
    dto.locationId = this.selectedLocation ? this.selectedLocation.id : '';
    this.deviceService.updateDevice(dto.id, dto).subscribe((result) => {
      this.dialogRef.close(result);
    });
  }

  valid(dto: DeviceDto) {
    return true;
  }
}
