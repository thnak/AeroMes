package com.iotviet.aeromes.di

import com.iotviet.aeromes.data.repository.AuthRepositoryImpl
import com.iotviet.aeromes.data.repository.InventoryRepositoryImpl
import com.iotviet.aeromes.data.repository.QualityRepositoryImpl
import com.iotviet.aeromes.data.repository.WorkRepositoryImpl
import com.iotviet.aeromes.domain.repository.AuthRepository
import com.iotviet.aeromes.domain.repository.InventoryRepository
import com.iotviet.aeromes.domain.repository.QualityRepository
import com.iotviet.aeromes.domain.repository.WorkRepository
import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class RepositoryModule {

    @Binds @Singleton
    abstract fun bindAuthRepository(impl: AuthRepositoryImpl): AuthRepository

    @Binds @Singleton
    abstract fun bindWorkRepository(impl: WorkRepositoryImpl): WorkRepository

    @Binds @Singleton
    abstract fun bindQualityRepository(impl: QualityRepositoryImpl): QualityRepository

    @Binds @Singleton
    abstract fun bindInventoryRepository(impl: InventoryRepositoryImpl): InventoryRepository
}
