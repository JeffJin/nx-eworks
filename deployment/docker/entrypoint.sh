#!/bin/sh

set -e

# mount the samba share
if [ ${ENABLE_SAMBA} -eq 1 ]; then
    /sbin/mount.cifs -o username="${SAMBA_USER}",password="${SAMBA_PASS}" //${SAMBA_HOST}/${SAMBA_SHARE} ${SAMBA_MOUNT_POINT}
fi

exec "$@"
