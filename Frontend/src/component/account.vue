<template>
	<b>{{ account.displayName }} (<money :currency-id="account.currency" :value="account.balance" />)</b>
	<router-link :to="`/addIncome/${brokerId}/${account.id}`" class="btn btn-primary ml-2">Add Income</router-link>
	<router-link :to="`/addExpense/${brokerId}/${account.id}`" class="btn btn-primary ml-2">Add Expense</router-link>
	<router-link :to="`/operations/${brokerId}/${account.id}`" class="btn btn-secondary ml-2">Operations</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { AccountDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';

@Options({
	name: 'Account',
	components: {
		Money,
	},
})
export default class Account extends Vue {
	@Prop()
	brokerId!: string;

	@Prop()
	account!: AccountDto;
}
</script>
