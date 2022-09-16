import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {FormsModule} from '@angular/forms';
import {AdworksMaterialModule} from '@app/adworks-material.module';

import {AddPipDialogComponent} from '@app/components/dialogs/add-pip/add-pip';
import {AddTextDialogComponent} from '@app/components/dialogs/add-text/add-text';
import {AddWeatherForecastDialogComponent} from '@app/components/dialogs/add-weather-forecast/add-weather-forecast';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {MergeAudioDialogComponent} from '@app/components/dialogs/merge-audio/merge-audio.component';
import {MergeImageDialogComponent} from '@app/components/dialogs/merge-image/merge-image.component';
import {AddGroupDialogComponent} from '@app/components/dialogs/create-group/add-group.component';
import {EditDeviceDialogComponent} from '@app/components/dialogs/edit-device/edit-device.component';
import {EditGroupDialogComponent} from '@app/components/dialogs/edit-group/edit-group.component';
import {RegisterDeviceDialogComponent} from '@app/components/dialogs/register-device/register-device.component';

@NgModule({
  declarations: [
    AddPipDialogComponent,
    AddTextDialogComponent,
    AddWeatherForecastDialogComponent,
    AddGroupDialogComponent,
    ConfirmDeleteDialogComponent,
    EditDeviceDialogComponent,
    EditGroupDialogComponent,
    MergeAudioDialogComponent,
    MergeImageDialogComponent,
    RegisterDeviceDialogComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    AdworksMaterialModule
  ]
})
export class DialogsModule { }
