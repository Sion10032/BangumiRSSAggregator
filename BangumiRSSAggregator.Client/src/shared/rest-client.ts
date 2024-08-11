import client from "./client"
import { ValueHttpResult } from "./types/responses"

export function getRestClient<T, TKey>(path : string) {
  const getAll = async () => {
    return (await client.get(path).json<ValueHttpResult<T[]>>()).value;
  }
  const get = async (id : TKey) => {
    return (await client
      .get(`${path}/${id}`)
      .json<ValueHttpResult<T>>()).value;
  };
  const add = async (obj : T) => {
    const resp = await client.post(path, { json: obj });
    return resp.ok;
  }
  const del = async (id : TKey) => {
    const resp = await client.delete(`${path}/${id}`);
    return resp.ok;
  }
  const update = async (id: TKey, obj : T) => {
    const resp = await client.put(`${path}/${id}`, { json: obj });
    return resp.ok;
  }

  return {
    getAll,
    get,
    add,
    del,
    update,
  }
}