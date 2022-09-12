import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {LocationDto} from '../../../../models/dtos';
import {LocationService} from '../../../../services/location.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-edit-location',
  templateUrl: './edit-location.component.html',
  styleUrls: ['./edit-location.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditLocationComponent implements OnInit {
  timezones: any;
  selectedTimezone: any;
  selectedLocale: any;

  constructor(private locationService: LocationService,
              public dialogRef: MatDialogRef<EditLocationComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit(): void {
    this.loadTimezones();
    this.initLocale();
  }

  cancel(): void {
    this.dialogRef.close();
  }

  initLocale(){
    this.selectedLocale = this.data.locales.find(t => {
      if(t.locale === this.data.dto.locale){
        return true;
      }
    });
  }

  loadTimezones(){
    return this.locationService.getTimezones().subscribe(results => {
      this.timezones = results;
      this.selectedTimezone = this.timezones.find(t => {
        if(t.offset === this.data.dto.timezoneOffset){
          return true;
        }
      });
      this.data.dto.timezoneOffset = this.selectedTimezone.offset;
    });
  }

  save(dto: LocationDto) {
    this.locationService.updateLocation(dto.id, dto).subscribe((result) => {
      this.dialogRef.close(result);
    });
  }

  timezoneSelected(timezone: any){
    console.log(timezone);
    this.data.dto.timezoneOffset = timezone.offset;
  }

  localeSelected(lo: any){
    this.data.dto.locale = lo.locale;
  }

}
