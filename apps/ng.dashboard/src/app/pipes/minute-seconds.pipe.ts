import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'minuteSeconds'
})
export class MinuteSecondsPipe implements PipeTransform {

  transform(value: number): string {
    const minutes: number = Math.floor(value / 60);
    const seconds: number = Math.round(value - minutes * 60);
    let min = '' + minutes;
    let sec = '' + seconds;
    if(seconds < 10){
      sec = '0' + seconds;
    }
    if(minutes < 10){
      min = '0' + minutes;
    }
    return min + ':' + sec;
  }

}
