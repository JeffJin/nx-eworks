import {Component, ElementRef, OnDestroy, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {HttpEventType, HttpResponse} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import {HubConnection, HubConnectionBuilder, LogLevel} from '@aspnet/signalr';
import {Utils} from '@app/services/utils';
import {AssetType, AudioDto, ImageDto, VideoDto} from '@app/models/dtos';
import {CacheService} from '@app/services/cache.service';
import {VideoService} from '@app/services/video.service';
import {DomSanitizer} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {FileService} from '@app/services/file.service';
import {ImageService} from '@app/services/image.service';
import {AudioService} from '@app/services/audio.service';
import {environment} from '@environments/environment';
import {MessageTopics} from '@app/constants/message.topics';
import * as humps from 'humps';
import {SignalrService} from '@app/services/signalr.service';
import {MatStepper} from '@angular/material/stepper';
import {CommonService} from '@app/services/common.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class UploadComponent implements OnInit, OnDestroy {
  @ViewChild('videoFile') videoFile: ElementRef;
  @ViewChild('uploadStepper') uploadStepper: MatStepper;

  error = '';
  fileType: AssetType;
  task = 'Uploading Asset';
  vodUrl = '';
  audioUrl = '';
  imageUrl = '';
  sanitizedAudioUrl = null;
  sanitizedVodUrl = null;
  isFileDragOver = false;
  fileUploadForm: UntypedFormGroup;
  settingsForm: UntypedFormGroup;
  progress = 0;
  selectedFile: File;
  thumbnails: string[] = [];
  mainThumbnail = '';
  videoDto: VideoDto = null;
  audioDto: AudioDto = null;
  imageDto: ImageDto = null;
  selectedCategory = null;
  categories = [];
  private hubConnection: HubConnection;


  constructor(private formBuilder: UntypedFormBuilder, private videoService: VideoService,
              private fileService: FileService, private imageService: ImageService,
              private audioService: AudioService, private sanitizer: DomSanitizer,
              private cacheService: CacheService, private router: Router,
              private commonService: CommonService, private signalRService: SignalrService,
              ) { }

  ngOnInit(): void {
    this.fileUploadForm = this.formBuilder.group({
      fileCtrl: ['', Validators.required]
    });
    this.settingsForm = this.formBuilder.group({
      titleCtrl: ['', Validators.required],
      descCtrl: [''],
      categoryCtrl: [''],
      tagsCtrl: ['']
    });

    this.startHubConnection();

    this.vodUrl = '';

    this.commonService.categories$.subscribe(results => {
      this.categories = results;
      setTimeout(() => {
        this.selectedCategory = this.categories.find(cat => cat.name === 'Advertisement');
        this.settingsForm.controls.categoryCtrl.setValue(this.selectedCategory);
      });
    });
  }

  ngOnDestroy(): void{
    this.stopHubConnection();
  }

  startHubConnection(): void {
    // const transportType = TransportType[this.utils.getParameterByName('transport')] || TransportType.LongPolling;

    const jwtToken = this.cacheService.getToken();
    this.hubConnection = this.signalRService.getHubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}eventing?access_token=${jwtToken}`)
      .configureLogging(LogLevel.Critical)
      .build();

    this.hubConnection.on('VideoEvent', (result: any) => {
      const temp = JSON.parse(result);
      const tempObj = JSON.parse(temp.Message);
      tempObj.Body = JSON.parse(tempObj.Body);
      const message = humps.camelizeKeys(tempObj);
      console.log('Message received', message);

      this.handleVideoMessageCallback(message);
    });

    this.hubConnection.on('FTPEvent', (result: any) => {
      const temp = JSON.parse(result);
      const tempObj = JSON.parse(temp.Message);
      tempObj.Body = JSON.parse(tempObj.Body);
      const message = humps.camelizeKeys(tempObj);
      console.log('Message received', message);
    });

    this.hubConnection.on('ImageEvent', (result: any) => {
      const temp = JSON.parse(result);
      const tempObj = JSON.parse(temp.Message);
      tempObj.Body = JSON.parse(tempObj.Body);
      const message = humps.camelizeKeys(tempObj);
      console.log('Message received', message);

      message.topic.startsWith('Image');
    });

    this.hubConnection.on('AudioEvent', (result: any) => {
      const temp = JSON.parse(result);
      const tempObj = JSON.parse(temp.Message);
      tempObj.Body = JSON.parse(tempObj.Body);
      const message = humps.camelizeKeys(tempObj);
      console.log('Message received', message);

      this.handleAudioMessageCallback(message);
    });

    this.hubConnection.start()
      .then(() => {
        console.log('Hub connection started');
      })
      .catch(err => {
        console.log('Error while establishing connection');
      });
  }

  handleVideoMessageCallback(message: any): void{
    const body = message.body;
    if (message.topic === MessageTopics.VideoStartProcessing){
      this.task = 'Processing Asset';
      this.progress = 0;
    }
    if (message.topic === MessageTopics.VideoEncode){
      this.task = 'Encoding Video';
      this.progress = body.progress;
    }
    else if (message.topic === MessageTopics.VideoFinishProcessing){
      this.videoDto = body;
      this.task = 'Video Processed';
      this.vodUrl = body.cloudUrl;
      this.sanitizedVodUrl = this.sanitizer.bypassSecurityTrustResourceUrl(body.cloudUrl);
      this.thumbnails = body.thumbnails;
      this.mainThumbnail = body.mainThumbnail;

      this.videoService.getThumbnails(body.id).subscribe(results => {
        this.thumbnails = results.slice(0, 4);
      });
    }
    else if (message.topic === MessageTopics.VideoFailedProcessing){
      this.videoDto = null;
      this.task = 'Failed to process video';
      this.vodUrl = '';
    }
    else if (message.topic === MessageTopics.VideoTransferToFtpSite){
      this.task = 'Publishing Video ' + body.sourceFile;
      this.progress = 0;
    }
    else if (message.topic === MessageTopics.VideoTransferToFtpSiteProgress){
      this.task = 'Publishing Video ' + body.jobName;
      this.progress = body.progress;
    }
    else if (message.topic === MessageTopics.VideoTransferToFtpSiteComplete){
      this.task = 'Publishing Video Complete';
      this.progress = 100;
    }
  }

  handleAudioMessageCallback(message: any): void{
    const body = message.body;
    if (message.topic === MessageTopics.AudioStartProcessing){
      this.task = 'Processing Audio';
      this.progress = 0;
    }
    if (message.topic === MessageTopics.AudioEncode){
      this.task = 'Encoding Audio';
      this.progress = body.progress;
    }
    else if (message.topic === MessageTopics.AudioFinishProcessing){
      this.audioDto = body;
      this.task = 'Audio Processed';
      this.audioUrl = body.cloudUrl;
      this.sanitizedAudioUrl = this.sanitizer.bypassSecurityTrustResourceUrl(body.cloudUrl);
    }
    else if (message.topic === MessageTopics.AudioFailedProcessing){
      this.audioDto = null;
      this.task = 'Failed to process audio';
      this.audioUrl = '';
    }
    else if (message.topic === MessageTopics.AudioTransferToFtpSite){
      this.task = 'Publishing Audio ' + body.sourceFile;
      this.progress = 0;
    }
    else if (message.topic === MessageTopics.AudioTransferToFtpSiteProgress){
      this.task = 'Publishing Audio ' + body.jobName;
      this.progress = body.progress;
    }
    else if (message.topic === MessageTopics.AudioTransferToFtpSiteComplete){
      this.task = 'Publishing Audio Complete';
      this.progress = 100;
    }
  }

  handleImageMessageCallback(message: any): void{
    const body = message.body;
    if (message.topic === MessageTopics.ImageStartProcessing){
      this.task = 'Processing Image';
      this.progress = 0;
    }
    else if (message.topic === MessageTopics.ImageFinishProcessing){
      this.imageDto = body;
      this.task = 'Image Processed';
      this.imageUrl = body.cloudUrl;
    }
    else if (message.topic === MessageTopics.ImageFailedProcessing){
      this.imageDto = null;
      this.task = 'Failed to process image';
      this.imageUrl = '';
    }
    else if (message.topic === MessageTopics.ImageTransferToFtpSite){
      this.task = 'Publishing Image ' + body.sourceFile;
      this.progress = 0;
    }
    else if (message.topic === MessageTopics.ImageTransferToFtpSiteProgress){
      this.task = 'Publishing Image ' + body.jobName;
      this.progress = body.progress;
    }
    else if (message.topic === MessageTopics.ImageTransferToFtpSiteComplete){
      this.task = 'Publishing Image Complete';
      this.progress = 100;
    }
  }

  stopHubConnection(): void{
    this.hubConnection.stop();
    console.log('Hub connection stopped');
  }

  triggerFileDialog(): void{
    this.videoFile.nativeElement.click();
  }

  onDragOver(ev): void{
    ev.preventDefault();
    ev.stopPropagation();
    this.isFileDragOver = true;
  }
  onDragLeave(ev): void{
    ev.preventDefault();
    ev.stopPropagation();
    this.isFileDragOver = false;
  }

  uploadFile(file: File): void{
    this.error = '';
    this.uploadStepper.next();
    this.settingsForm.controls.titleCtrl.setValue(file.name.split('.')[0]);
    if (Utils.isVideo(file.name)){
      this.fileType = 'Video';
    }
    else if (Utils.isImage(file.name)){
      this.fileType = 'Image';
    }
    else if (Utils.isAudio(file.name)){
      this.fileType = 'Audio';
    }

    if (Utils.isMediaFile(file.name)){
      this.uploadFileToServer(file);
    }
    else{
      this.error = 'The file format ' + Utils.getExtension(file.name) + ' is not supported';
    }
  }

  onDropFile(ev): void{
    ev.preventDefault();
    // If dropped items aren't files, reject them
    const dt = ev.dataTransfer;
    if (dt.items) {
      // Use DataTransferItemList interface to access the file(s)
      this.selectedFile = dt.items[0].getAsFile();
    } else {
      // Use DataTransfer interface to access the file(s)
      this.selectedFile = dt.files[0];
    }
    this.uploadFile(this.selectedFile);
  }

  onFileSelected(): void{
    const fileList: FileList = this.videoFile.nativeElement.files;
    if (fileList && fileList[0]){
      this.selectedFile = fileList[0];
      this.uploadFile(this.selectedFile);
    }
  }

  submitSettingsForm(form: UntypedFormGroup): void {
    if (this.fileType === 'Video') {
      this.videoDto.title = form.controls.titleCtrl.value;
      this.videoDto.description = form.controls.descCtrl.value;
      this.videoDto.tags = form.controls.tagsCtrl.value;
      this.videoDto.category = this.selectedCategory.name;
      this.videoService.updateVideo(this.videoDto.id, this.videoDto).subscribe((result) => {
          setTimeout(() => {
            return this.router.navigateByUrl('/dashboard/assets/videos');
          });

        },
        (err) => {
          console.error(err);
        });
    }
    else if (this.fileType === 'Audio') {
      this.audioDto.title = form.controls.titleCtrl.value;
      this.audioDto.description = form.controls.descCtrl.value;
      this.audioDto.tags = form.controls.tagsCtrl.value;
      this.audioDto.category = this.selectedCategory.name;
      this.audioService.updateAudio(this.audioDto.id, this.audioDto).subscribe((result) => {
          setTimeout(() => {
            return this.router.navigateByUrl('/dashboard/assets/audios');
          });
        },
        (err) => {
          console.error(err);
        });
    }
    else if (this.fileType === 'Image') {
      this.imageDto.title = form.controls.titleCtrl.value;
      this.imageDto.description = form.controls.descCtrl.value;
      this.imageDto.tags = form.controls.tagsCtrl.value;
      this.imageDto.category = this.selectedCategory.name;
      this.imageService.updateImage(this.imageDto.id, this.imageDto).subscribe((result) => {
          setTimeout(() => {
            return this.router.navigateByUrl('/dashboard/assets/images');
          });
        },
        (err) => {
          console.error(err);
        });
    }

  }

  uploadFileToServer(file: File): void {
    const formData: FormData = new FormData();
    formData.append(file.name, file, file.name);
    this.fileService.uploadFile(formData).subscribe(
        evt => {
          if (evt.type === HttpEventType.UploadProgress) {
            // This is an upload progress event. Compute and show the % done:
            const percentDone = Math.round(100 * evt.loaded / evt.total);
            this.progress = percentDone;
            console.log(`File is ${percentDone}% uploaded.`);
          } else if (evt instanceof HttpResponse) {
            this.progress = 100;
            console.log('File is completely uploaded!');
          }
        },
        error => {
          console.log(error);
          Observable.throw(error);
        }
      );
  }


}
