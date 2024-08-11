export function cloneAndUpdateProperty<TObject extends object, TPropertyName extends keyof TObject>(obj : TObject, propertyName: TPropertyName, newValue : TObject[TPropertyName]) {
  const newObj = { ...obj };
  newObj[propertyName] = newValue;
  return newObj;
}
