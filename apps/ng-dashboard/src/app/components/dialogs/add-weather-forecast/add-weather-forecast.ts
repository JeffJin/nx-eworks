import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {Component, Inject} from '@angular/core';

@Component({
  selector: 'app-add-weather-forecast',
  templateUrl: 'add-weather-forecast.html',
  styleUrls: ['./add-weather-forecast.scss']
})
export class AddWeatherForecastDialogComponent{

  constructor(
    public dialogRef: MatDialogRef<AddWeatherForecastDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

}
