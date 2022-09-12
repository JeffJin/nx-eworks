import {ElementRef, NgModule} from '@angular/core';
import {LoadingDirective} from '@app/directives/loading/loading.directive';


@NgModule({
  declarations: [
    LoadingDirective,
  ],
  imports: [

  ],
})
export class DirectivesModule {
  constructor(private el: ElementRef) {
    this.el.nativeElement.style.backgroundColor = 'grey';
  }
}
