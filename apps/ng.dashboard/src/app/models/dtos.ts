import {Utils} from '@app/services/utils';

export class EntityDto {
  id?: string;
  createdOn?: Date;
  updatedOn?: Date;
  createdBy?: string;
  updatedBy?: string;
  visibleId?: string;

  update(updater?: string): this {
    if (updater) {
      this.updatedBy = updater;
    }
    this.updatedOn = new Date();
    return this;
  }
}


export class ImageDto extends EntityDto {
  encodedFilePath: string;
  cloudUrl: string;
  category: string;
  type: string;
  title: string;
  description: string;
  tags: string;
  checked?: boolean;

  public constructor(init?: Partial<ImageDto>) {
    super();
    Object.assign(this, init);
  }

  serialize(): this {
    this.description = Utils.sanitizeString(this.description);
    this.title = Utils.sanitizeString(this.title);
    return this;
  }
}

export class VideoDto extends EntityDto {
  cloudUrl: string;
  encodedFilePath: string;
  progressiveUrl: string;
  hlsUrl: string;
  duration: number;
  sourceId: string;
  sourceType: string;
  category: string;
  type: string;
  title: string;
  description: string;
  tags: string;
  mainThumbnail: string;
  thumbnails: string[];
  checked?: boolean;

  public constructor(init?: Partial<VideoDto>) {
    super();
    Object.assign(this, init);
  }

  serialize(): this {
    this.description = Utils.sanitizeString(this.description);
    this.title = Utils.sanitizeString(this.title);
    this.checked = null;
    return this;
  }
}

export class AudioDto extends EntityDto {
  encodedFilePath: string;
  cloudUrl: string;
  duration: number;
  category: string;
  type: string;
  title: string;
  description: string;
  tags: string;

  public constructor(init?: Partial<AudioDto>) {
    super();
    Object.assign(this, init);
  }

  serialize(): this {
    this.description = Utils.sanitizeString(this.description);
    this.title = Utils.sanitizeString(this.title);
    return this;
  }
}

export class DeviceDto extends EntityDto {
  serialNumber: string;
  deviceGroupName: string;
  organizationName: string;
  assetTag: string;
  deviceVersion: number;
  appVersion: number;
  locationId: string;
  activatedOn?: Date;
  lastStatus?: DeviceStatusDto;
  isOnline?: boolean;
  licenses?: Array<LicenseDto>;
  customerAssetTag?: string;

  public constructor(init?: Partial<DeviceDto>) {
    super();
    Object.assign(this, init);
  }
}

export class CategoryDto {
  id: string;
  name: string;
  subCategories?: [];
}

export class LicenseDto extends EntityDto {
  deviceId: string;
  type: string;
  expireOn: Date;

  public constructor(init?: Partial<LicenseDto>) {
    super();
    Object.assign(this, init);
  }
}


export class DeviceStatusDto extends EntityDto {
  deviceId: string;
  status: string;

  public constructor(init?: Partial<DeviceStatusDto>) {
    super();
    Object.assign(this, init);
  }
}

export class GroupDto extends EntityDto {
  name: string;
  numOfDevices?: number;
  numOfPlaylists?: number;

  public constructor(init?: Partial<GroupDto>) {
    super();
    Object.assign(this, init);
  }
}

export class PlaylistDto extends EntityDto {
  name: string;
  startDate: Date;
  endDate: Date;
  // daily start and end time in minutes, offset from midnight
  startTime: number;
  endTime: number;
  deviceGroups: GroupDto[];
  subPlaylists: SubPlaylistDto[];

  public constructor(init?: Partial<PlaylistDto>) {
    super();
    Object.assign(this, init);
  }
}


export class SubPlaylistDto extends EntityDto {
  playlistId: string;
  positionX: number; // top left corner in a screen
  positionY: number; // top left corner in a screen
  width: number; // 0 to 100 percentage
  height: number; // 0 to 100 percentage
  playlistItems: PlaylistItemDto[];

  public constructor(init?: Partial<SubPlaylistDto>) {
    super();
    Object.assign(this, init);
  }
}

export class PlaylistItemDto extends EntityDto {
  index: number;
  subPlaylistId: string;
  mediaAssetId: string; // media asset id
  assetDiscriminator: string; // Video, Audio or Image
  duration: number; // for how long the asset will be displayed since the media start time
  media: Asset;
  cacheLocation: string;

  public constructor(init?: Partial<PlaylistItemDto>) {
    super();
    Object.assign(this, init);
  }
}

export class CustomerDto extends EntityDto {
  name: string;

  public constructor(init?: Partial<CustomerDto>) {
    super();
    Object.assign(this, init);
  }
}

export class LocationDto extends EntityDto {
  address: string;
  locale: string;
  timezoneOffset: number;

  public constructor(init?: Partial<LocationDto>) {
    super();
    Object.assign(this, init);
  }
}

export class PlaylistGroupDto extends EntityDto {
  playlistId: string;
  groupId: string;
}

export class UserDto {
  userName: string;
  email: string;
  phoneNumber: string;
  token?: string;
}

export type Asset = ImageDto | VideoDto | AudioDto;

export type AssetType = 'Video' | 'Image' | 'Audio';

export type Client = CustomerDto & LocationDto;

// export class ClientInfo implements Client {
//   address: string;
//   locale: string;
//   name: string;
//   timezoneOffset: number;
//   update: ((updater?: string) => this);
// }
