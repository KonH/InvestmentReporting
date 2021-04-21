<template>
	<td>{{ displayDate }}</td>
	<td>{{ operation.kind }}</td>
	<td>{{ operation.category }}</td>
	<td>{{ operation.brokerName }}</td>
	<td>{{ operation.accountName }}</td>
	<td>{{ operation.assetIsin }}</td>
	<td><money-value-diff :currency-code="operation.currency" :old="0" :new="operation.amount" /></td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import OperationData from '@/dto/operationData';
import { Prop } from 'vue-property-decorator';
import MoneyValueDiff from '@/component/common/moneyValueDiff.vue';

@Options({
	name: 'Operation',
	components: {
		MoneyValueDiff,
	},
})
export default class Operation extends Vue {
	@Prop()
	operation!: OperationData;

	get displayDate() {
		return new Date(this.operation.date ?? '').toUTCString();
	}
}
</script>
