package com.iotviet.aeromes.ui.quality

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.InspectionOrder
import com.iotviet.aeromes.domain.repository.QualityRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class QualityListUiState(
    val orders: List<InspectionOrder> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

data class QualityDetailUiState(
    val order: InspectionOrder? = null,
    val isLoading: Boolean = false,
    val isActing: Boolean = false,
    val error: String? = null,
    val actionSuccess: Boolean = false
)

@HiltViewModel
class QualityViewModel @Inject constructor(
    private val qualityRepository: QualityRepository
) : ViewModel() {

    private val _listState = MutableStateFlow(QualityListUiState())
    val listState = _listState.asStateFlow()

    private val _detailState = MutableStateFlow(QualityDetailUiState())
    val detailState = _detailState.asStateFlow()

    init { loadOrders() }

    fun refresh() = loadOrders()

    private fun loadOrders() {
        viewModelScope.launch {
            _listState.update { it.copy(isLoading = true, error = null) }
            qualityRepository.getInspectionOrders()
                .onSuccess { orders -> _listState.update { it.copy(orders = orders, isLoading = false) } }
                .onFailure { e -> _listState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }

    fun loadDetail(id: Int) {
        viewModelScope.launch {
            _detailState.update { it.copy(isLoading = true, error = null) }
            qualityRepository.getInspectionOrder(id)
                .onSuccess { order -> _detailState.update { it.copy(order = order, isLoading = false) } }
                .onFailure { e -> _detailState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }

    fun startInspection(id: Int) = act { qualityRepository.startInspection(id) }
    fun passInspection(id: Int) = act { qualityRepository.passInspection(id) }
    fun failInspection(id: Int) = act { qualityRepository.failInspection(id) }

    private fun act(block: suspend () -> Result<Unit>) {
        viewModelScope.launch {
            _detailState.update { it.copy(isActing = true, error = null) }
            block()
                .onSuccess { _detailState.update { it.copy(isActing = false, actionSuccess = true) } }
                .onFailure { e -> _detailState.update { it.copy(isActing = false, error = e.message) } }
        }
    }
}
