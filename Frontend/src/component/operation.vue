<template>
	<td>{{ operation.date }}</td>
	<td><money :currency-id="operation.currency" :value="operation.amount" /></td>
	<td>{{ operation.kind }}</td>
	<td>{{ operation.category }}</td>
	<td v-if="operation.asset">{{ operation.asset }}</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { OperationDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';

@Options({
	name: 'Operation',
	components: {
		Money,
	},
})
export default class Operation extends Vue {
	@Prop()
	operation!: OperationDto;
}
</script>
