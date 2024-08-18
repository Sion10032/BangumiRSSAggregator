import ky, { Options } from "ky";

import config from "@/config";

const option : Options = {
  prefixUrl: config.BaseUrl,
  timeout: 20000,
};

const client = ky.extend(option);

export default client;