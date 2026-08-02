import {
    registerVerifiedLoader,
    VERIFIED_LOADER_MANIFEST_ENV,
    VERIFIED_LOADER_STATIC_ROOT_ENV,
} from "./verified-loader.mjs";

const staticRoot = process.env[VERIFIED_LOADER_STATIC_ROOT_ENV];
const manifestPath = process.env[VERIFIED_LOADER_MANIFEST_ENV];
if (staticRoot === undefined || manifestPath === undefined) {
    throw new Error(
        `Verified loader requires ${VERIFIED_LOADER_STATIC_ROOT_ENV} and ${VERIFIED_LOADER_MANIFEST_ENV}.`,
    );
}

registerVerifiedLoader({ staticRoot, manifestPath });
