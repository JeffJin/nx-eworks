import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {CustomerDto, DeviceDto, GroupDto, LocationDto} from '@app/models/dtos';
import {GroupService} from '@app/services/group.service';
import {CustomerService} from '@app/services/customer.service';
import {LocationService} from '@app/services/location.service';

@Component({
  selector: 'app-register-device-dialog',
  templateUrl: './register-device.component.html',
  styleUrls: ['./register-device.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class RegisterDeviceDialogComponent implements OnInit {
  selectedGroup: GroupDto;
  selectedCustomer: CustomerDto;
  selectedLocation: LocationDto;
  groups: any[];
  customers: any[];
  locations: any[];

  constructor(private groupService: GroupService, private customerService: CustomerService,
              public dialogRef: MatDialogRef<RegisterDeviceDialogComponent>, private locationService: LocationService,
              @Inject(MAT_DIALOG_DATA) public data: DeviceDto) { }

  ngOnInit(): void {
    this.loadGroups();
    this.loadCustomers();
    this.loadLocations();
  }

  loadCustomers(){
    this.customerService.getCustomers().subscribe(results => {
      this.customers = results;
    });
  }

  loadGroups(){
    this.groupService.loadGroups().subscribe(results => {
      this.groups = results;
    });
  }

  loadLocations(){
    this.locationService.loadLocations().subscribe(results => {
      this.locations = results;
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(dto: DeviceDto): void {
    if(!dto.serialNumber){
      return;
    }
    if(this.selectedGroup){
      dto.deviceGroupName = this.selectedGroup.name;
    }
    if(this.selectedCustomer){
      dto.organizationName = this.selectedCustomer.name;
    }
    if(this.selectedLocation){
      dto.locationId = this.selectedLocation.id;
    }
    this.dialogRef.close(dto);
  }

  valid(dto: DeviceDto): boolean{
    return !!dto.serialNumber;
  }

}
