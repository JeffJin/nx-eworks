import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {LocationDto} from '../../../../models/dtos';
import {LocationService} from '../../../../services/location.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-add-location',
  templateUrl: './add-location.component.html',
  styleUrls: ['./add-location.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AddLocationComponent implements OnInit {
  timezones: any;
  selectedTimezone: any;
  selectedLocale: any;

  constructor(public dialogRef: MatDialogRef<AddLocationComponent>, private locationService: LocationService,
              @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit(): void {
    this.loadTimezones();
  }

  loadTimezones(){
    return this.locationService.getTimezones().subscribe(results => {
      this.timezones = results;
      // const currentOffset = Intl.DateTimeFormat().resolvedOptions().timeZone;
      const offsetMinutes = new Date().getTimezoneOffset();
      const currentOffset = -(offsetMinutes / 60);
      this.selectedTimezone = this.timezones.find(t => {
        if(t.offset === currentOffset){
          return true;
        }
      });
      this.data.dto.timezoneOffset = this.selectedTimezone.offset;
    });
  }

  timezoneSelected(timezone: any){
    console.log(timezone);
    this.data.dto.timezoneOffset = timezone.offset;
  }

  localeSelected(lo: any){
    this.data.dto.locale = lo.locale;
  }

  cancel(): void {
    this.dialogRef.close();
  }

}
