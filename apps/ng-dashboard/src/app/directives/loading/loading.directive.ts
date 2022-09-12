import {Directive, ElementRef} from '@angular/core';

@Directive({
  selector: '[appLoading]'
})
export class LoadingDirective {

  constructor(private el: ElementRef) { }

}
