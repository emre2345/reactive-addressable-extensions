using System;
using UniRx;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace ReactiveAddressableExtensions
{
    public static class AssetReferenceExtensions
    {
        public static IObservable<UnityEngine.Object> GetOrLoadAsObservable(this AssetReference reference)
        {
            if (reference.Asset != null)
                return Observable.Return(reference.Asset);
            else
            {
                return Observable.Create(delegate(IObserver<Object> observer)
                {
                    return reference.LoadAssetAsync<Object>().ToObservable().Subscribe(delegate(Unit unit)
                    {
                        observer.OnNext(reference.Asset);
                        observer.OnCompleted();
                    });
                });
            }
        }

        public static IObservable<T> GetOrLoadAsObservable<T>(this AssetReference reference)
            where T : UnityEngine.Object
        {
            if (reference.Asset != null)
                return Observable.Return(reference.Asset as T);
            else
            {
                return Observable.Create(delegate(IObserver<T> observer)
                {
                    var handle = reference.LoadAssetAsync<T>();
                    return handle.ToObservable().Subscribe(delegate(Unit unit)
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                            observer.OnNext(handle.Result);
                        else
                        {
                            observer.OnError(new Exception($"Could not load the asset, {handle.DebugName}"));
                        }

                        observer.OnCompleted();
                    });
                });
            }
        }

        public static IObservable<T> AsObsevable<T>(this AsyncOperationHandle<T> asyncOperationHandle)
            where T : UnityEngine.Object
        {
            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                return Observable.Return(asyncOperationHandle.Result);
            else
            {
                return asyncOperationHandle.ToObservable().Select(unit => asyncOperationHandle.Result);
            }
        }
    }
}