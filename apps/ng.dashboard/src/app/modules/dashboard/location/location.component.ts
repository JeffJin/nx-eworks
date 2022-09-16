import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {LocationDto} from '@app/models/dtos';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {LocationService} from '@app/services/location.service';
import {AddLocationComponent} from './add-location/add-location.component';
import {EditLocationComponent} from './edit-location/edit-location.component';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-location',
  templateUrl: './location.component.html',
  styleUrls: ['./location.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class LocationComponent implements OnInit {
  locations: any[];
  timezones: any[] = [];
  keywords = '';
  selectedFilter = 'createdOn';
  locales = [
    {
      locale: 'en-us',
      text: 'English (US)'
    },
    {
      locale: 'en-ca',
      text: 'English (Canada)'
    },
    {
      locale: 'fr-ca',
      text: 'French (Canada)'
    },
  ];
  tableData: TableData;

  constructor(private locationService: LocationService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.loadLocations();
    this.loadTimezones();
  }

  loadTimezones(): any{
    return this.locationService.getTimezones().subscribe(results => {
      this.timezones = results;
    });
  }

  search(key): void{
    this.locationService.searchLocations(key).subscribe(results => {
      this.locations = results.map(v => {
        v.checked = false;
        return v;
      });
    });
  }

  addNew(): void{
    const dto = new LocationDto({
      address: null,
      locale: null,
      timezoneOffset: null,
      createdOn: new Date()
    });
    const dialogRef = this.dialog.open(AddLocationComponent, {
      width: '360px',
      data: { dto, locales: this.locales }
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        this.locationService.addLocation(data.dto).subscribe(results => {
            this.loadLocations();
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

  loadLocations(): void{
    this.locationService.loadLocations().subscribe(results => {
      // refresh the result
      this.locations = results.map(v => {
        v.checked = false;
        return v;
      });

      this.tableData = {
        title: 'All locations',
        template: 'locationTemplate',
        headerRow: ['Address', 'Locale', 'Timezone', 'Created On'],
        dataRows: this.locations
      };
    });
  }

  toggle(item): void{
    item.checked = !item.checked;
  }

  edit(id: string): void{
    this.locationService.getLocation(id).subscribe(dto => {
      const dialogRef = this.dialog.open(EditLocationComponent, {
        width: '360px',
        data: {dto, locales: this.locales}
      });

      dialogRef.afterClosed().subscribe(data => {
        if (data && data.id) {
          for (let i = 0; i < this.locations.length; i++){
            if (this.locations[i].id === data.id){
              this.locations[i] = data;
            }
          }
        }
      });
    });
  }

  getLocaleInfo(locale: string): any{
    const lo = this.locales.find(r => r.locale === locale);
    if (lo){
      return lo.text;
    }
    return '';
  }

  getTimezoneInfo(offset: number): any{
    const zone = this.timezones.find(r => r.offset === offset);
    if (zone){
      return zone.value;
    }
    return '';
  }

  remove(dto): void{
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.address, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue'){
        this.locationService.deleteLocation(dto.id).subscribe(result => {
          this.loadLocations();
          this.snackBar.open('Location with address \'' + dto.address + '\' has been successfully removed.', 'SUCCESS', {
            duration: 3000,
            panelClass: ['action-success']
          });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove location with address \'' + dto.address + '\'', 'FAILURE', {
            duration: 5000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }
}
