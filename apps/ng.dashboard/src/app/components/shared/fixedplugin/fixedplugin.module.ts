import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FixedPluginComponent } from './fixedplugin.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [FixedPluginComponent],
  exports: [FixedPluginComponent]
})
export class FixedPluginModule { }
