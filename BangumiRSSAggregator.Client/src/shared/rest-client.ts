import client from "./client"
import { ValueHttpResult } from "./types/responses"

export function getRestClient<T, TKey>(path : string) {
  const c = client.extend(options => ({
    ...options,
    prefixUrl: `${options.prefixUrl}/${path}`,
  }));
  const getAll = async () => {
    return (await c.get("").json<ValueHttpResult<T[]>>()).value;
  }
  const get = async (id : TKey) => {
    return (await c
      .get(`${id}`)
      .json<ValueHttpResult<T>>()).value;
  };
  const add = async (obj : T) => {
    const resp = await c.post("", { json: obj });
    return resp.ok;
  }
  const del = async (id : TKey) => {
    const resp = await c.delete(`${id}`);
    return resp.ok;
  }
  const update = async (id: TKey, obj : T) => {
    const resp = await c.put(`${id}`, { json: obj });
    return resp.ok;
  }

  return {
    getAll,
    get,
    add,
    del,
    update,
    client: c,
  }
}